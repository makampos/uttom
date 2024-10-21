using MediatR;
using Uttom.Domain.Results;
using System.Text.Json.Serialization;

namespace Uttom.Application.Features.Commands;

public record AddDelivererCommand(
    [property: JsonPropertyName("identificador")] string Identifier,
    [property: JsonPropertyName("nome")] string Name,
    [property: JsonPropertyName("cnpj")] string BusinessTaxId,
    [property: JsonPropertyName("data_nascimento")] DateTime DateOfBirth,
    [property: JsonPropertyName("numero_cnh")] string DriverLicenseNumber,
    [property: JsonPropertyName("tipo_cnh")] int DriverLicenseType,
    [property: JsonPropertyName("imagem_cnh")] string? DriverLicenseImageBase64String) : IRequest<ResultResponse<bool>>;