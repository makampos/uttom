using MediatR;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class AddRentalCommandHandler : IRequestHandler<AddRentalCommand, ResultResponse<bool>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public AddRentalCommandHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<bool>> Handle(AddRentalCommand command, CancellationToken cancellationToken)
    {
        // get plan
        // TODO: Add Validator on API level to check this
        var plan = RentalPlans.GetPlan(command.PlanId);

        if (plan is null)
        {
            return ResultResponse<bool>.FailureResult("Plan not found.");
        }

        // can not rent using past date
        // TODO: Add Validator on API level to check this
        if (command.StartDate <= DateOnly.FromDateTime(DateTime.Now).AddDays(-1))
        {
            return ResultResponse<bool>.FailureResult("The start date must be today or a future date.");
        }

        var endDate = command.StartDate.AddDays(plan.Days);

        // Get motorcycle
        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(command.MotorcycleId, cancellationToken);

        if (motorcycle is null)
        {
            return ResultResponse<bool>.FailureResult("Motorcycle not found.");
        }

        // check delivererId
        var deliverer = await _uttomUnitOfWork.DelivererRepository.GetByIdAsync(command.DeliverId, cancellationToken);

        if (deliverer is null)
        {
            return ResultResponse<bool>.FailureResult("Deliverer not found.");
        }

        if (deliverer.DriverLicenseType is DriverLicenseType.B)
        {
            return ResultResponse<bool>.FailureResult("Deliverer must have a driver license type A.");
        }

        // add Rental
       var rental = Rental.Create(
           planId: command.PlanId,
           endDate: endDate ,
           estimatingEndingDate: command.EstimatingEndingDate,
           delivererId: command.DeliverId,
           motorcycleId: command.MotorcycleId);

        await _uttomUnitOfWork.RentalRepository.AddAsync(rental, cancellationToken);
        await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

        return ResultResponse<bool>.SuccessResult(true);
    }
}