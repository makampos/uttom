using Uttom.Application.DTOs;
using Uttom.Domain.Models;

namespace Uttom.Application.Extensions;

public static class Mappers
{
    public static RentalDto ToDto(this Rental rental, decimal dailyRate)
    {
        return new RentalDto(
            rental.Id,
            string.Empty, // choose which identifier to use, motorcycle, deliverer or it is own Rental identifier?
            dailyRate,
            rental.DelivererId,
            rental.MotorcycleId,
            rental.StartDate,
            rental.EndDate,
            rental.EstimatingEndingDate
        );
    }
}