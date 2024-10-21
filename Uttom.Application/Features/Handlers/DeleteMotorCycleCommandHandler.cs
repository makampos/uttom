using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class DeleteMotorCycleCommandHandler : IRequestHandler<DeleteMotorcycleCommand, ResultResponse<bool>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ILogger<DeleteMotorCycleCommandHandler> _logger;

    public DeleteMotorCycleCommandHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<DeleteMotorCycleCommandHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger; // Initialize logger
    }

    public async Task<ResultResponse<bool>> Handle(DeleteMotorcycleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(request.Id, cancellationToken);
            if (motorcycle is null)
            {
                _logger.LogWarning("Attempted to delete a motorcycle that was not found: {MotorcycleId}", request.Id);
                return ResultResponse<bool>.FailureResult("Motorcycle not found.");
            }

            var rental = await _uttomUnitOfWork.RentalRepository.GetByMotorcycleIdAsync(motorcycle.Id, cancellationToken);
            if (rental is not null)
            {
                _logger.LogWarning("Cannot delete motorcycle with ID: {MotorcycleId} because it has an active rental record.", motorcycle.Id);
                return ResultResponse<bool>.FailureResult("Motorcycle has rental record.");
            }

            await _uttomUnitOfWork.MotorcycleRepository.DeleteAsync(motorcycle, cancellationToken);
            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully deleted motorcycle with ID: {MotorcycleId}", motorcycle.Id);
            return ResultResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting motorcycle with ID: {MotorcycleId}. Error: {Message}", request.Id, ex.Message);
            return ResultResponse<bool>.FailureResult("An unexpected error occurred.");
        }
    }
}