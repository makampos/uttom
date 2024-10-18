using Uttom.Domain.Models;

namespace Uttom.Domain.Interfaces.Repositories;

public interface IMotorcycleRepository : IRepository<Motorcycle>
{
    Task<Motorcycle?> GetByPlateNumberAsync(string plateNumber, bool isDeleted, CancellationToken cancellationToken = default);
}