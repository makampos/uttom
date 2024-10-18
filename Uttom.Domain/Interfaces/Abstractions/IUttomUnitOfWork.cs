using Uttom.Domain.Interfaces.Repositories;

namespace Uttom.Domain.Interfaces.Abstractions;

public interface IUttomUnitOfWork : IUnitOfWork
{
    public IMotorcycleRepository MotorcycleRepository { get; init; }
    public IRegisteredMotorCycleRepository RegisteredMotorCyclesRepository { get; init; }

    public IDelivererRepository DelivererRepository { get; init; }

    public IRentalRepository RentalRepository { get; init; }
}