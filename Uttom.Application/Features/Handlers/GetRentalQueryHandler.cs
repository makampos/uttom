using MediatR;
using Microsoft.Extensions.Logging;
using Uttom.Application.DTOs;
using Uttom.Application.Extensions;
using Uttom.Application.Features.Queries;
using Uttom.Domain.Enum;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Handlers;

public class GetRentalQueryHandler : IRequestHandler<GetRentalQuery, ResultResponse<RentalDto>>
{
    private readonly IUttomUnitOfWork _uttomUnitOfWork;
    private readonly ILogger<GetRentalQueryHandler> _logger;

    public GetRentalQueryHandler(IUttomUnitOfWork uttomUnitOfWork, ILogger<GetRentalQueryHandler> logger)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
        _logger = logger;
    }

    public async Task<ResultResponse<RentalDto>> Handle(GetRentalQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving rental with ID: {RentalId}", request.RentalId);

            var rental = await _uttomUnitOfWork.RentalRepository.GetByIdWithIncludeAsync(request.RentalId, cancellationToken);

            if (rental is null)
            {
                _logger.LogWarning("Rental not found for ID: {RentalId}", request.RentalId);
                return ResultResponse<RentalDto>.FailureResult("Rental not found.");
            }

            var dailyRate = RentalPlans.GetPrice(rental.PlanId);
            var rentalDto = rental.ToDto(dailyRate);

            _logger.LogInformation("Successfully retrieved rental for ID: {RentalId}", request.RentalId);
            return ResultResponse<RentalDto>.SuccessResult(rentalDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving rental with ID: {RentalId}. Error: {Message}", request.RentalId, ex.Message);
            return ResultResponse<RentalDto>.FailureResult("An unexpected error occurred.");
        }
    }
}