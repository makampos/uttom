using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetTotalRentalPriceQueryHandler : IRequestHandler<GetTotalRentalPriceQuery, ResultResponse<decimal>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly RentalCalculator _rentalCalculator = new();
    private readonly ILogger<GetTotalRentalPriceQueryHandler> _logger;

    public GetTotalRentalPriceQueryHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<GetTotalRentalPriceQueryHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger;
    }

    public async Task<ResultResponse<decimal>> Handle(GetTotalRentalPriceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Calculating total rental price for Rental ID: {RentalId}", request.RentalId);

            var rental = await _uttomUnitOfWork.RentalRepository.GetByIdAsync(request.RentalId, cancellationToken);
            if (rental is null)
            {
                _logger.LogWarning("Rental not found for ID: {RentalId}", request.RentalId);
                return ResultResponse<decimal>.FailureResult("Rental not found.");
            }

            if (request.ActualReturnDate < rental.StartDate)
            {
                _logger.LogWarning("Actual return date: {ActualReturnDate} cannot be before the rental start date: {StartDate}.", request.ActualReturnDate, rental.StartDate);
                return ResultResponse<decimal>.FailureResult("Actual return date cannot be before the rental start date.");
            }

            var plan = RentalPlans.GetPlan(rental.PlanId);
            if (plan is null)
            {
                _logger.LogWarning("Plan not found for Plan ID: {PlanId}", rental.PlanId);
                return ResultResponse<decimal>.FailureResult("Plan not found.");
            }

            var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(
                plannedReturnDate: new DateTime(rental.EndDate.Year, rental.EndDate.Month, rental.EndDate.Day),
                actualReturnDate: new DateTime(request.ActualReturnDate.Year, request.ActualReturnDate.Month, request.ActualReturnDate.Day),
                dailyRate: plan.Price,
                rentalPlanDays: plan.Days);

            _logger.LogInformation("Successfully calculated total rental price for Rental ID: {RentalId}. Total Price: {TotalPrice}", request.RentalId, totalPrice);
            return ResultResponse<decimal>.SuccessResult(totalPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while calculating total rental price for Rental ID: {RentalId}. Error: {Message}", request.RentalId, ex.Message);
            return ResultResponse<decimal>.FailureResult("An unexpected error occurred.");
        }
    }
}