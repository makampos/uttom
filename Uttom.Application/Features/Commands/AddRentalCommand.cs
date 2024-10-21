using MediatR;
using Uttom.Domain.Results;
using System.Text.Json.Serialization;

namespace Uttom.Application.Features.Commands;

public record AddRentalCommand(
    [property: JsonPropertyName("plano")] int PlanId,
    [property: JsonPropertyName("entregador_id")] int DeliverId,
    [property: JsonPropertyName("moto_id")] int MotorcycleId,
    [property: JsonPropertyName("data_inicio")] DateOnly StartDate,
    [property: JsonPropertyName("data_previsao_termino")] DateOnly EstimatingEndingDate) : IRequest<ResultResponse<bool>>;