using MediatR;
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

    public GetRentalQueryHandler(IUttomUnitOfWork uttomUnitOfWork)
    {
        _uttomUnitOfWork = uttomUnitOfWork;
    }

    public async Task<ResultResponse<RentalDto>> Handle(GetRentalQuery request, CancellationToken cancellationToken)
    {
        var rental = await _uttomUnitOfWork.RentalRepository.GetByIdWithIncludeAsync(request.RentalId, cancellationToken);

        if (rental is null)
        {
            return ResultResponse<RentalDto>.FailureResult("Rental not found.");
        }

        var dailyRate = RentalPlans.GetPrice(rental.PlanId);

        var rentalDto = rental.ToDto(dailyRate);

        return ResultResponse<RentalDto>.SuccessResult(rentalDto);
    }
}