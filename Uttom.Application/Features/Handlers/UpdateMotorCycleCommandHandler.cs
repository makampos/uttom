using MediatR;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class UpdateMotorCycleCommandHandler : IRequestHandler<UpdateMotorcycleCommand, ResultResponse<string>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public UpdateMotorCycleCommandHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<string>> Handle(UpdateMotorcycleCommand command, CancellationToken cancellationToken)
    {
        var motorcycleId = command.MotorcycleId ?? 0;
        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(motorcycleId, cancellationToken);

        if (motorcycle is null)
        {
            return ResultResponse<string>.FailureResult("Motorcycle not found.");
        }

        motorcycle.Update(command.PlateNumber);

        await _uttomUnitOfWork.MotorcycleRepository.UpdateAsync(motorcycle, cancellationToken);

        await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

        return ResultResponse<string>.SuccessResult("Motorcycle updated successfully.");
    }
}