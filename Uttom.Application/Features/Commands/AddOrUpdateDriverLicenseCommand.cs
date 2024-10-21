using System.Text.Json.Serialization;
using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record AddOrUpdateDriverLicenseCommand(
    [property: JsonPropertyName("imagem_cnh")] string DriverLicenseImageBase64,
    [property: JsonIgnore] int? DelivererId = null): IRequest<ResultResponse<bool>>
{
    public AddOrUpdateDriverLicenseCommand AddDelivererId(int delivererId)
    {
        return this with { DelivererId = delivererId };
    }
}




