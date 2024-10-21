using Uttom.Application.DTOs;
using Uttom.Domain.Messages;
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

    public static MotorcycleDto ToDto(this Motorcycle motorcycle)
    {
        return new MotorcycleDto(
            motorcycle.Id,
            motorcycle.Identifier,
            motorcycle.Year,
            motorcycle.Model,
            motorcycle.PlateNumber
        );
    }

    public static RegisteredMotorcycleDto ToDto(this RegisteredMotorcycle registeredMotorcycle)
    {
        return new RegisteredMotorcycleDto(
            registeredMotorcycle.Id,
            registeredMotorcycle.Identifier,
            registeredMotorcycle.Year,
            registeredMotorcycle.Model,
            registeredMotorcycle.PlateNumber
        );
    }
}