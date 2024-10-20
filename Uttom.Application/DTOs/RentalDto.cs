using System.Text.Json.Serialization;

namespace Uttom.Application.DTOs;

public record RentalDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("identificador")] string Identifier,
    [property: JsonPropertyName("valor_diaria")] decimal DailyRate,
    [property: JsonPropertyName("entregador_id")] int DelivererId,
    [property: JsonPropertyName("moto_id")] int MotorcycleId,
    [property: JsonPropertyName("data_inicio")] DateOnly StartDate,
    [property: JsonPropertyName("data_termino")] DateOnly EndDate,
    [property: JsonPropertyName("data_previsao_termino")] DateOnly EstimatingEndingDate);
