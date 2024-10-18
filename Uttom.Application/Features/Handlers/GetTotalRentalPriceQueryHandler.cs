using MediatR;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetTotalRentalPriceQueryHandler : IRequestHandler<CalculateTotalRentalPriceQuery, ResultResponse<decimal>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly RentalCalculator _rentalCalculator = new();

    public GetTotalRentalPriceQueryHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<decimal>> Handle(CalculateTotalRentalPriceQuery request, CancellationToken cancellationToken)
    {
        var rental = await _uttomUnitOfWork.RentalRepository.GetByIdAsync(request.RentalId, cancellationToken);

        if (rental is null)
        {
            return ResultResponse<decimal>.FailureResult("Rental not found.");
        }

        var plan = RentalPlans.GetPlan(rental.PlanId);

        if ( plan is null)
        {
            return ResultResponse<decimal>.FailureResult("Plan not found.");
        }

        var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(
            plannedReturnDate: new DateTime(rental.EndDate.Year, rental.EndDate.Month, rental.EndDate.Day), // Make it better
            actualReturnDate: new DateTime(request.ActualReturnDate.Year, request.ActualReturnDate.Month, request.ActualReturnDate.Day), // Make it better
            dailyRate: plan.Price,
            rentalPlanDays: plan.Days);

        return ResultResponse<decimal>.SuccessResult(totalPrice);
    }
}