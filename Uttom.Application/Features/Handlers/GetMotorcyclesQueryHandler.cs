using MediatR;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetMotorcyclesQueryHandler : IRequestHandler<GetMotorcyclesQuery, ResultResponse<PagedResult<Motorcycle>>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public GetMotorcyclesQueryHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<PagedResult<Motorcycle>>> Handle(GetMotorcyclesQuery request, CancellationToken cancellationToken)
    {
        var motorcycles = await _uttomUnitOfWork.MotorcycleRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);

        return ResultResponse<PagedResult<Motorcycle>>.SuccessResult(motorcycles);
    }
}