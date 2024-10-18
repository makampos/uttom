using MediatR;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetMotorCycleByPlateNumberQueryHandler : IRequestHandler<GetMotorcycleByPlateNumberQuery, ResultResponse<Motorcycle>>
{

    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public GetMotorCycleByPlateNumberQueryHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<Motorcycle>> Handle(GetMotorcycleByPlateNumberQuery request, CancellationToken cancellationToken)
    {

        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(request.PlateNumber, false, cancellationToken);

        if (motorcycle is null)
        {
            return ResultResponse<Motorcycle>.FailureResult("Motorcycle not found.");
        }

        return ResultResponse<Motorcycle>.SuccessResult(motorcycle);
    }
}