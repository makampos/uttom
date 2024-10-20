using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record AddOrUpdateDriverLicenseCommand(string DriverLicenseImageBase64, int? DelivererId = null): IRequest<ResultResponse<bool>>
{
    public AddOrUpdateDriverLicenseCommand AddDelivererId(int delivererId)
    {
        return this with { DelivererId = delivererId };
    }
}




