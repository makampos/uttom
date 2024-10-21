using System.Text.Json.Serialization;

namespace Uttom.Application.DTOs;

public record MotorcycleDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("identificador")] string Identifier,
    [property: JsonPropertyName("ano")] int Year,
    [property: JsonPropertyName("modelo")] string Model,
    [property: JsonPropertyName("placa")] string PlateNumber);