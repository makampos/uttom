using MediatR;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<AddOrUpdateDriverLicenseCommandHandler> _logger;

    public AddOrUpdateDriverLicenseCommandHandler(IUttomUnitOfWork unitOfWork, IMinioService minioService, IImageService imageService, ILogger<AddOrUpdateDriverLicenseCommandHandler> logger)
    {
        _uttomUnitOfWork = unitOfWork;
        _minioService = minioService;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<ResultResponse<bool>> Handle(AddOrUpdateDriverLicenseCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var delivererId = command.DelivererId ?? 0;

            var deliverer = await _uttomUnitOfWork.DelivererRepository.GetByIdAsync(delivererId, cancellationToken);
            if (deliverer is null)
            {
                _logger.LogWarning("Deliverer not found for ID: {DelivererId}", delivererId);
                return ResultResponse<bool>.FailureResult("Deliverer not found.");
            }

            var isValidExtension = _imageService.ValidateImageExtension(command.DriverLicenseImageBase64);
            if (!isValidExtension)
            {
                _logger.LogWarning("Invalid image extension for Driver License Image.");
                return ResultResponse<bool>.FailureResult("The image extension is not valid.");
            }

            var driverLicenseImageId = await _minioService.UploadImageAsync(deliverer.Id, command.DriverLicenseImageBase64);
            deliverer.AddOrUpdateDriverLicenseImageId(driverLicenseImageId);

            await _uttomUnitOfWork.DelivererRepository.UpdateAsync(deliverer, cancellationToken);
            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated driver license for Deliverer ID: {DelivererId}", delivererId);
            return ResultResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding or updating the driver license: {Message}", ex.Message);
            return ResultResponse<bool>.FailureResult("An unexpected error occurred.");
        }
    }
}