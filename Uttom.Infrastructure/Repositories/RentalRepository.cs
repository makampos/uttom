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
}