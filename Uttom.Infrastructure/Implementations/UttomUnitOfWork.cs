using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Interfaces.Repositories;

namespace Uttom.Infrastructure.Implementations;

public class UttomUnitOfWork(
    ApplicationDbContext applicationDbContext,
    IMotorcycleRepository motorcycleRepository,
    IRegisteredMotorCycleRepository registeredMotorCyclesRepository,
    IDelivererRepository delivererRepository,
    IRentalRepository rentalRepository
) : UnitOfWork(applicationDbContext), IUttomUnitOfWork
{
    public IMotorcycleRepository MotorcycleRepository { get; init; } = motorcycleRepository;
    public IRegisteredMotorCycleRepository RegisteredMotorCyclesRepository { get; init; } = registeredMotorCyclesRepository;
    public IDelivererRepository DelivererRepository { get; init; } = delivererRepository;
    public IRentalRepository RentalRepository { get; init; } = rentalRepository;
}