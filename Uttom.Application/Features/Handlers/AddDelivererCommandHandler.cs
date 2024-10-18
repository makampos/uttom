using MediatR;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Services;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class AddDelivererCommandHandler : IRequestHandler<AddDelivererCommand, ResultResponse<bool>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly IMinioService _minioService;
    private readonly IImageService _imageService;

    public AddDelivererCommandHandler(IUttomUnitOfWork uttomUnitOfWork, IMinioService minioService, IImageService imageService)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _minioService = minioService;
        _imageService = imageService;
    }

    public async Task<ResultResponse<bool>> Handle(AddDelivererCommand command, CancellationToken cancellationToken)
    {
        var deliverer = Deliverer.Create(
            command.Identifier,
            command.Name,
            command.BusinessTaxId,
            command.DateOfBirth,
            command.DriverLicenseNumber,
            (DriverLicenseType)command.DriverLicenseType);

        var existingBusinessTaxId = await _uttomUnitOfWork.DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId, cancellationToken);

        if (existingBusinessTaxId is not null)
        {
            return ResultResponse<bool>.FailureResult("The business tax id must be unique.");
        }

        var existingDriverLicenseNumber = await _uttomUnitOfWork.DelivererRepository.GetDelivererByDriverLicenseNumberAsync(command.DriverLicenseNumber, cancellationToken);

        if (existingDriverLicenseNumber is not null)
        {
            return ResultResponse<bool>.FailureResult("The driver license number must be unique.");
        }

        await _uttomUnitOfWork.DelivererRepository.AddAsync(deliverer, cancellationToken);

        await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

        // TODO: Implement feature to store deliverer's driver license photo
        // Add file extension validation on API level

        if (!string.IsNullOrEmpty(command.DriverLicenseImageBase64String))
        {
            var isValidExtension = _imageService.ValidateImageExtension(command.DriverLicenseImageBase64String);

            if (!isValidExtension)
            {
                return ResultResponse<bool>.FailureResult("The image extension is not valid.");
            }

            var objectName = await _minioService.UploadImageAsync(deliverer.Id, command.DriverLicenseImageBase64String);

            deliverer.AddOrUpdateDriverLicenseImageId(objectName);

            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);
        }

        return ResultResponse<bool>.SuccessResult(true);
    }
}