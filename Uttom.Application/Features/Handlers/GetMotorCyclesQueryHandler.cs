using MediatR;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetMotorCyclesQueryHandler : IRequestHandler<GetMotorcyclesQuery, ResultResponse<PagedResult<Motorcycle>>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public GetMotorCyclesQueryHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<PagedResult<Motorcycle>>> Handle(GetMotorcyclesQuery request, CancellationToken cancellationToken)
    {
        var motorcycles = await _uttomUnitOfWork.MotorcycleRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);

        if (motorcycles.TotalCount == 0)
        {
            return ResultResponse<PagedResult<Motorcycle>>.FailureResult("Motorcycles not found.");
        }

        return ResultResponse<PagedResult<Motorcycle>>.SuccessResult(motorcycles);
    }
}