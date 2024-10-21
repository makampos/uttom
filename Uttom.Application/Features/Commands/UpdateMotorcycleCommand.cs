using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;


using System.Text.Json.Serialization;

public record UpdateMotorcycleCommand(
    [property: JsonPropertyName("placa")] string PlateNumber,
    [property: JsonIgnore] int? MotorcycleId = null) : IRequest<ResultResponse<string>>
{
    public UpdateMotorcycleCommand WithMotorcycleId(int motorcycleId) => this with { MotorcycleId = motorcycleId };
}