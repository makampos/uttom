using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record UpdateRentalCommand(DateOnly ReturnDate, int? RentalId = null) : IRequest<ResultResponse<string>>
{
    public UpdateRentalCommand WithRentalId(int rentalId) => this with { RentalId = rentalId };
}