using MediatR;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Services;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class AddOrUpdateDriverLicenseCommandHandler : IRequestHandler<AddOrUpdateDriverLicenseCommand, ResultResponse<bool>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly IMinioService _minioService;
    private readonly IImageService _imageService;

    public AddOrUpdateDriverLicenseCommandHandler(IUttomUnitOfWork unitOfWork, IMinioService minioService, IImageService imageService)
    {
        _uttomUnitOfWork = unitOfWork;
        _minioService = minioService;
        _imageService = imageService;
    }

    public async Task<ResultResponse<bool>> Handle(AddOrUpdateDriverLicenseCommand command, CancellationToken cancellationToken)
    {
        var delivererId = command.DelivererId ?? 0;

        var deliverer = await _uttomUnitOfWork.DelivererRepository.GetByIdAsync(delivererId, cancellationToken);
        if (deliverer is null)
        {
            return ResultResponse<bool>.FailureResult("Deliverer not found.");
        }

        var isValidExtension = _imageService.ValidateImageExtension(command.DriverLicenseImageBase64);

        if (!isValidExtension)
        {
            return ResultResponse<bool>.FailureResult("The image extension is not valid.");
        }

        var driverLicenseImageId = await _minioService.UploadImageAsync(deliverer.Id, command.DriverLicenseImageBase64);

        deliverer.AddOrUpdateDriverLicenseImageId(driverLicenseImageId);

        await _uttomUnitOfWork.DelivererRepository.UpdateAsync(deliverer, cancellationToken);

        await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

        return ResultResponse<bool>.SuccessResult(true);
    }
}