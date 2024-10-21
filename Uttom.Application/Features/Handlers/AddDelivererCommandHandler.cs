using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<AddDelivererCommandHandler> _logger;

    public AddDelivererCommandHandler(IUttomUnitOfWork uttomUnitOfWork, IMinioService minioService, IImageService imageService, ILogger<AddDelivererCommandHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _minioService = minioService;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<ResultResponse<bool>> Handle(AddDelivererCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var deliverer = Deliverer.Create(
                command.Identifier,
                command.Name,
                command.BusinessTaxId,
                command.DateOfBirth,
                command.DriverLicenseNumber,
                (DriverLicenseType)command.DriverLicenseType);

            _logger.LogInformation("Creating deliverer with BusinessTaxId: {BusinessTaxId}", command.BusinessTaxId);

            var existingBusinessTaxId = await _uttomUnitOfWork.DelivererRepository.GetDelivererByBusinessTaxIdAsync(command.BusinessTaxId, cancellationToken);
            if (existingBusinessTaxId is not null)
            {
                _logger.LogWarning("Business tax ID already exists: {BusinessTaxId}", command.BusinessTaxId);
                return ResultResponse<bool>.FailureResult("The business tax id must be unique.");
            }

            var existingDriverLicenseNumber = await _uttomUnitOfWork.DelivererRepository.GetDelivererByDriverLicenseNumberAsync(command.DriverLicenseNumber, cancellationToken);
            if (existingDriverLicenseNumber is not null)
            {
                _logger.LogWarning("Driver license number already exists: {DriverLicenseNumber}", command.DriverLicenseNumber);
                return ResultResponse<bool>.FailureResult("The driver license number must be unique.");
            }

            await _uttomUnitOfWork.DelivererRepository.AddAsync(deliverer, cancellationToken);
            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

            if (!string.IsNullOrEmpty(command.DriverLicenseImageBase64String))
            {
                var isValidExtension = _imageService.ValidateImageExtension(command.DriverLicenseImageBase64String);
                if (!isValidExtension)
                {
                    _logger.LogWarning("Invalid image extension for Driver License Image.");
                    return ResultResponse<bool>.FailureResult("The image extension is not valid.");
                }

                var objectName = await _minioService.UploadImageAsync(deliverer.Id, command.DriverLicenseImageBase64String);
                deliverer.AddOrUpdateDriverLicenseImageId(objectName);
                await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Uploaded driver license image with object name: {ObjectName}", objectName);
            }

            return ResultResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a deliverer: {Message}", ex.Message);
            return ResultResponse<bool>.FailureResult("An unexpected error occurred");
        }
    }
}