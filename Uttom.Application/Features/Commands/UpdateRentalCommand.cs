using MediatR;
using Uttom.Domain.Results;
using System.Text.Json.Serialization;

namespace Uttom.Application.Features.Commands;

public record UpdateRentalCommand(
    [property: JsonPropertyName("data_devolucao")] DateOnly ReturnDate,
    [property: JsonIgnore] int? RentalId = null) : IRequest<ResultResponse<string>>
{
    public UpdateRentalCommand WithRentalId(int rentalId) => this with { RentalId = rentalId };
}