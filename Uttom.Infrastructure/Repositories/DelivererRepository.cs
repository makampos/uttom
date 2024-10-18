using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;

namespace Uttom.Infrastructure.Repositories;

public class DelivererRepository
    (ApplicationDbContext applicationDbContext) : Repository<Deliverer>(applicationDbContext), IDelivererRepository
{
    public async Task<Deliverer?> GetDelivererByBusinessTaxIdAsync(string businessTaxId, CancellationToken cancellationToken = default)
    {
        return await applicationDbContext.Deliverers
            .FirstOrDefaultAsync(x => x.BusinessTaxId == businessTaxId, cancellationToken);
    }

    public async Task<Deliverer?> GetDelivererByDriverLicenseNumberAsync(string driverLicenseNumber, CancellationToken cancellationToken = default)
    {
        return await applicationDbContext.Deliverers
            .FirstOrDefaultAsync(x => x.DriverLicenseNumber == driverLicenseNumber, cancellationToken);
    }
}