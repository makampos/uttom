using Uttom.Domain.Messages;

namespace Uttom.Domain.Interfaces.Repositories;

public interface IRegisteredMotorCycleRepository : IRepository<RegisteredMotorcycle>
{
    Task<RegisteredMotorcycle?> GetByPlateNumberAsync(string plateNumber, CancellationToken cancellationToken = default);
}