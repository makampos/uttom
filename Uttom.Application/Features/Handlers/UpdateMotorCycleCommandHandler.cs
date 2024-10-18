using MediatR;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class UpdateMotorCycleCommandHandler : IRequestHandler<UpdateMotorcycleCommand, ResultResponse<Motorcycle>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public UpdateMotorCycleCommandHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<Motorcycle>> Handle(UpdateMotorcycleCommand command, CancellationToken cancellationToken)
    {
        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(command.Id, cancellationToken);

        if (motorcycle is null)
        {
            return ResultResponse<Motorcycle>.FailureResult("Motorcycle not found.");
        }

        motorcycle.Update(command.PlateNumber);

        await _uttomUnitOfWork.MotorcycleRepository.UpdateAsync(motorcycle, cancellationToken);

        await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

        return ResultResponse<Motorcycle>.SuccessResult(motorcycle);
    }
}