using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Implementations;

namespace Uttom.Infrastructure.Repositories;

public class MotorcycleRepository(
    ApplicationDbContext applicationDbContext
) : Repository<Motorcycle>(applicationDbContext), IMotorcycleRepository
{
    public async Task<Motorcycle?> GetByPlateNumberAsync(string plateNumber, bool isDeleted, CancellationToken cancellationToken = default)
    {
        return await applicationDbContext.Motorcycles.FirstOrDefaultAsync(x => x.PlateNumber == plateNumber && x.IsDeleted == isDeleted, cancellationToken);
    }
}