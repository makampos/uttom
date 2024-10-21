using MediatR;
using Uttom.Application.DTOs;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetMotorcyclesQueryHandler : IRequestHandler<GetMotorcyclesQuery, ResultResponse<PagedResult<MotorcycleDto>>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;

    public GetMotorcyclesQueryHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<PagedResult<MotorcycleDto>>> Handle(GetMotorcyclesQuery request, CancellationToken cancellationToken)
    {
        var motorcycles = await _uttomUnitOfWork.MotorcycleRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);

       var motorcyclesDto = motorcycles.ToDto(x => x.ToDto());

        return ResultResponse<PagedResult<MotorcycleDto>>.SuccessResult(motorcyclesDto);
    }
}