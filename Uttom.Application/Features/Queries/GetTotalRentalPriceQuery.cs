using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Queries;

public record GetTotalRentalPriceQuery(
    int RentalId,
    DateOnly ActualReturnDate) : IRequest<ResultResponse<decimal>>
{
    public GetTotalRentalPriceQuery WithRentalId(int rentalId)
    {
        return this with { RentalId = rentalId };
    }
}

public record GetTotalRentalPriceQueryString(DateOnly DataDevolucao) : IRequest<ResultResponse<decimal>>;