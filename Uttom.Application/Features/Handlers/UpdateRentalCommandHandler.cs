using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class UpdateRentalCommandHandler : IRequestHandler<UpdateRentalCommand, ResultResponse<string>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly RentalCalculator _rentalCalculator = new();
    private readonly ILogger<UpdateRentalCommandHandler> _logger;

    public UpdateRentalCommandHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<UpdateRentalCommandHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger;
    }

    public async Task<ResultResponse<string>> Handle(UpdateRentalCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var rentalId = command.RentalId ?? 0;
            _logger.LogInformation("Updating rental with ID: {RentalId}", rentalId);

            var rental = await _uttomUnitOfWork.RentalRepository.GetByIdWithIncludeAsync(rentalId, cancellationToken);
            if (rental is null)
            {
                _logger.LogWarning("Rental not found for ID: {RentalId}", rentalId);
                return ResultResponse<string>.FailureResult("Rental not found.");
            }

            if (command.ReturnDate < rental.StartDate)
            {
                _logger.LogWarning("Return date: {ReturnDate} cannot be before the rental start date: {StartDate}.", command.ReturnDate, rental.StartDate);
                return ResultResponse<string>.FailureResult("Return date cannot be before the rental start date.");
            }

            var plan = RentalPlans.GetPlan(rental.PlanId);
            if (plan is null)
            {
                _logger.LogWarning("Plan not found for Plan ID: {PlanId}", rental.PlanId);
                return ResultResponse<string>.FailureResult("Plan not found.");
            }

            var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(
                plannedReturnDate: new DateTime(rental.EndDate.Year, rental.EndDate.Month, rental.EndDate.Day),
                actualReturnDate: new DateTime(command.ReturnDate.Year, command.ReturnDate.Month, command.ReturnDate.Day),
                dailyRate: plan.Price,
                rentalPlanDays: plan.Days);

            rental.UpdateReturnDate(command.ReturnDate);
            await _uttomUnitOfWork.RentalRepository.UpdateAsync(rental, cancellationToken);
            await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully updated rental with ID: {RentalId}. Total Price: {TotalPrice}", rentalId, totalPrice);
            return ResultResponse<string>.SuccessResult($"Return date informed successfully and the total price for rental is {totalPrice}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating rental with ID: {RentalId}. Error: {Message}", command.RentalId, ex.Message);
            return ResultResponse<string>.FailureResult("An unexpected error occurred.");
        }
    }
}