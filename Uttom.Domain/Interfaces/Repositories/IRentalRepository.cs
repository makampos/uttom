using Uttom.Domain.Models;

namespace Uttom.Domain.Interfaces.Repositories;

public interface IRentalRepository : IRepository<Rental>
{
    Task<Rental?> GetByMotorcycleIdAsync(int motorcycleId, CancellationToken cancellationToken = default);
    Task<Rental?> GetByIdWithIncludeAsync(int id, CancellationToken cancellationToken = default);
    Task<Rental?> GetByDelivererIdAsync(int delivererId, CancellationToken cancellationToken = default);
}