using Uttom.Domain.Models;

namespace Uttom.Domain.Interfaces.Repositories;

public interface IDelivererRepository : IRepository<Deliverer>
{
    Task<Deliverer?> GetDelivererByBusinessTaxIdAsync(string businessTaxId, CancellationToken cancellationToken = default);
    Task<Deliverer?> GetDelivererByDriverLicenseNumberAsync(string driverLicenseNumber, CancellationToken cancellationToken = default);
}