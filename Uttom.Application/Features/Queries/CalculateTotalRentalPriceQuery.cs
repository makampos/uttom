using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Queries;

public record CalculateTotalRentalPriceQuery(int RentalId, DateOnly ActualReturnDate) : IRequest<ResultResponse<decimal>>;