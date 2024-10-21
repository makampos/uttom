using MediatR;
using Uttom.Domain.Results;
using System.Text.Json.Serialization;

namespace Uttom.Application.Features.Commands;

public record AddMotorcycleCommand(
    [property: JsonPropertyName("identificador")] string Identifier,
    [property: JsonPropertyName("ano")] int Year,
    [property: JsonPropertyName("modelo")] string Model,
    [property: JsonPropertyName("placa")] string PlateNumber) : IRequest<ResultResponse<bool>>;