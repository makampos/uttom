using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;

namespace Uttom.Infrastructure.Repositories;

public class RentalRepository(ApplicationDbContext applicationDbContext)
    : Repository<Rental>(applicationDbContext), IRentalRepository
{
    public async Task<Rental?> GetByMotorcycleIdAsync(int motorcycleId, CancellationToken cancellationToken = default)
    {
        return await SetAsNoTracking.FirstOrDefaultAsync(x => x.MotorcycleId == motorcycleId, cancellationToken);
    }

    public async Task<Rental?> GetByIdWithIncludeAsync(int id, CancellationToken cancellationToken = default)
    {
        return await SetAsTracking.Include(x => x.Motorcycle)
            .Include(x => x.Deliverer)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Rental?> GetByDelivererIdAsync(int delivererId, CancellationToken cancellationToken = default)
    {
        return await SetAsNoTracking.FirstOrDefaultAsync(x => x.DelivererId == delivererId, cancellationToken);
    }
}