using MediatR;
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

    public UpdateRentalCommandHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<string>> Handle(UpdateRentalCommand command, CancellationToken cancellationToken)
    {
        var rentalId = command.RentalId ?? 0;

       var rental = await _uttomUnitOfWork.RentalRepository.GetByIdWithIncludeAsync(rentalId, cancellationToken);

       if (rental is null)
       {
           return ResultResponse<string>.FailureResult("Rental not found.");
       }

       if (command.ReturnDate < rental.StartDate)
       {
           return ResultResponse<string>.FailureResult("Return date cannot be before the rental start date.");
       }

       var plan = RentalPlans.GetPlan(rental.PlanId)!;

       var totalPrice = _rentalCalculator.CalculateTotalRentalPrice(
           plannedReturnDate: new DateTime(rental.EndDate.Year, rental.EndDate.Month, rental.EndDate.Day), // Make it better
           actualReturnDate: new DateTime(command.ReturnDate.Year, command.ReturnDate.Month, command.ReturnDate.Day), // Make it better
           dailyRate: plan.Price,
           rentalPlanDays: plan.Days);

       rental.UpdateReturnDate(command.ReturnDate);

       await _uttomUnitOfWork.RentalRepository.UpdateAsync(rental, cancellationToken);
       await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

       return ResultResponse<string>.SuccessResult($"Return date informed successfully and the total price for rental is {totalPrice}");
    }
}