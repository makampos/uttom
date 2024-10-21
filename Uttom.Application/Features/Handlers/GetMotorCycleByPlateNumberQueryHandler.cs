using MediatR;
using Uttom.Application.DTOs;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetMotorCycleByPlateNumberQueryHandler : IRequestHandler<GetMotorcycleByPlateNumberQuery, ResultResponse<MotorcycleDto>>
{

    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public GetMotorCycleByPlateNumberQueryHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<MotorcycleDto>> Handle(GetMotorcycleByPlateNumberQuery request, CancellationToken cancellationToken)
    {

        var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(request.PlateNumber, false, cancellationToken);

        if (motorcycle is null)
        {
            return ResultResponse<MotorcycleDto>.FailureResult("Motorcycle not found.");
        }

        var motorcycleDto = motorcycle.ToDto();

        return ResultResponse<MotorcycleDto>.SuccessResult(motorcycleDto);
    }
}