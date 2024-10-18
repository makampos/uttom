using MediatR;
using Uttom.Application.Features.Commands;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class DeleteMotorCycleCommandHandler : IRequestHandler<DeleteMotorcycleCommand, ResultResponse<bool>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public DeleteMotorCycleCommandHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<bool>> Handle(DeleteMotorcycleCommand request, CancellationToken cancellationToken)
    {
        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByIdAsync(request.Id, cancellationToken);

        if (motorcycle == null)
        {
            return ResultResponse<bool>.FailureResult("Motorcycle not found.");
        }

        var rental = await _uttomUnitOfWork.RentalRepository.GetByMotorcycleIdAsync(motorcycle.Id, cancellationToken);

        if (rental != null)
        {
            return ResultResponse<bool>.FailureResult("Motorcycle has rental record.");
        }

        await _uttomUnitOfWork.MotorcycleRepository.DeleteAsync(motorcycle, cancellationToken);
        await _uttomUnitOfWork.SaveChangesAsync(cancellationToken);

        return ResultResponse<bool>.SuccessResult(true);
    }
}