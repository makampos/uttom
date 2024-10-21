using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class UpdateMotorCycleCommandHandler : IRequestHandler<UpdateMotorcycleCommand, ResultResponse<string>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ILogger<UpdateMotorCycleCommandHandler> _logger;

    public UpdateMotorCycleCommandHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<UpdateMotorCycleCommandHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger;
    }

    public async Task<ResultResponse<string>> Handle(UpdateMotorcycleCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var motorcycleId = command.MotorcycleId ?? 0;
            _logger.LogInformation("Updating motorcycle with ID: {MotorcycleId}", motorcycleId);

            var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(motorcycleId, cancellationToken);
            if (motorcycle is null)
            {
                _logger.LogWarning("Motorcycle not found for ID: {MotorcycleId}", motorcycleId);
                return ResultResponse<string>.FailureResult("Motorcycle not found.");
            }

            motorcycle.Update(command.PlateNumber);
            await _uttomUnitOfWork.MotorcycleRepository.UpdateAsync(motorcycle, cancellationToken);
            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated motorcycle with ID: {MotorcycleId}", motorcycleId);
            return ResultResponse<string>.SuccessResult("Motorcycle updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating motorcycle with ID: {MotorcycleId}. Error: {Message}", command.MotorcycleId, ex.Message);
            return ResultResponse<string>.FailureResult("An unexpected error occurred.");
        }
    }
}