using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.DTOs;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetMotorcyclesQueryHandler : IRequestHandler<GetMotorcyclesQuery, ResultResponse<PagedResult<MotorcycleDto>>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ILogger<GetMotorcyclesQueryHandler> _logger;

    public GetMotorcyclesQueryHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<GetMotorcyclesQueryHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger;
    }

    public async Task<ResultResponse<PagedResult<MotorcycleDto>>> Handle(GetMotorcyclesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving motorcycles for page number: {PageNumber}, page size: {PageSize}", request.PageNumber, request.PageSize);

            var motorcycles = await _uttomUnitOfWork.MotorcycleRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);
            var motorcyclesDto = motorcycles.ToDto(x => x.ToDto());

            _logger.LogInformation("Successfully retrieved {Count} motorcycles.", motorcyclesDto.Items.Count);
            return ResultResponse<PagedResult<MotorcycleDto>>.SuccessResult(motorcyclesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving motorcycles. Error: {Message}", ex.Message);
            return ResultResponse<PagedResult<MotorcycleDto>>.FailureResult("An unexpected error occurred.");
        }
    }
}