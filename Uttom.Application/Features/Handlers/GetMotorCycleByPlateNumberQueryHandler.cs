using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.DTOs;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetMotorCycleByPlateNumberQueryHandler : IRequestHandler<GetMotorcycleByPlateNumberQuery, ResultResponse<MotorcycleDto>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ILogger<GetMotorCycleByPlateNumberQueryHandler> _logger;

    public GetMotorCycleByPlateNumberQueryHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<GetMotorCycleByPlateNumberQueryHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger;
    }

    public async Task<ResultResponse<MotorcycleDto>> Handle(GetMotorcycleByPlateNumberQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var motorcycle = await _uttomUnitOfWork.MotorcycleRepository.GetByPlateNumberAsync(request.PlateNumber, false, cancellationToken);

            if (motorcycle is null)
            {
                _logger.LogWarning("Motorcycle not found for plate number: {PlateNumber}", request.PlateNumber);
                return ResultResponse<MotorcycleDto>.FailureResult("Motorcycle not found.");
            }

            var motorcycleDto = motorcycle.ToDto();
            _logger.LogInformation("Successfully retrieved motorcycle for plate number: {PlateNumber}", request.PlateNumber);

            return ResultResponse<MotorcycleDto>.SuccessResult(motorcycleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving motorcycle by plate number: {PlateNumber}. Error: {Message}", request.PlateNumber, ex.Message);
            return ResultResponse<MotorcycleDto>.FailureResult("An unexpected error occurred.");
        }
    }
}