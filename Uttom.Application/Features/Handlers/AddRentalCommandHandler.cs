using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class AddRentalCommandHandler : IRequestHandler<AddRentalCommand, ResultResponse<bool>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ILogger<AddRentalCommandHandler> _logger;

    public AddRentalCommandHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<AddRentalCommandHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger;
    }

    public async Task<ResultResponse<bool>> Handle(AddRentalCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var plan = RentalPlans.GetPlan(command.PlanId);
            if (plan is null)
            {
                _logger.LogWarning("Plan not found for ID: {PlanId}", command.PlanId);
                return ResultResponse<bool>.FailureResult("Plan not found.");
            }

            if (command.StartDate <= DateOnly.FromDateTime(DateTime.Now).AddDays(-1))
            {
                _logger.LogWarning("Invalid start date: {StartDate}. It must be today or a future date.", command.StartDate);
                return ResultResponse<bool>.FailureResult("The start date must be today or a future date.");
            }

            var endDate = command.StartDate.AddDays(plan.Days);

            var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(command.MotorcycleId, cancellationToken);
            if (motorcycle is null)
            {
                _logger.LogWarning("Motorcycle not found for ID: {MotorcycleId}", command.MotorcycleId);
                return ResultResponse<bool>.FailureResult("Motorcycle not found.");
            }

            var deliverer = await _uttomUnitOfWork.DelivererRepository.GetByIdAsync(command.DeliverId, cancellationToken);
            if (deliverer is null)
            {
                _logger.LogWarning("Deliverer not found for ID: {DelivererId}", command.DeliverId);
                return ResultResponse<bool>.FailureResult("Deliverer not found.");
            }

            if (deliverer.DriverLicenseType is DriverLicenseType.B)
            {
                _logger.LogWarning("Deliverer with ID: {DelivererId} has an invalid driver license type: {DriverLicenseType}.", command.DeliverId, deliverer.DriverLicenseType);
                return ResultResponse<bool>.FailureResult("Deliverer must have a driver license type A.");
            }

            var rental = Rental.Create(
                planId: command.PlanId,
                endDate: endDate,
                estimatingEndingDate: command.EstimatingEndingDate,
                delivererId: command.DeliverId,
                motorcycleId: command.MotorcycleId);

            await _uttomUnitOfWork.RentalRepository.AddAsync(rental, cancellationToken);
            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully added rental for Motorcycle ID: {MotorcycleId} by Deliverer ID: {DelivererId}", command.MotorcycleId, command.DeliverId);
            return ResultResponse<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a rental: {Message}", ex.Message);
            return ResultResponse<bool>.FailureResult("An unexpected error occurred.");
        }
    }
}