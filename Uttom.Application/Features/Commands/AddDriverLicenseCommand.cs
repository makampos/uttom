using MediatR;
using Uttom.Domain.Results;

namespace Uttom.Application.Features.Commands;

public record AddDriverLicenseCommand(string DriverLicenseImageBase64, int? DelivererId = null): IRequest<ResultResponse<bool>>
{
    public AddDriverLicenseCommand AddDelivererId(int delivererId)
    {
        return this with { DelivererId = delivererId };
    }
}




