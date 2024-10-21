using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Messages;
using Uttom.Infrastructure.Implementations;

namespace Uttom.Infrastructure.Repositories;

public class RegisteredMotorCycleRepository(ApplicationDbContext applicationDbContext)
    : Repository<RegisteredMotorcycle>(applicationDbContext), IRegisteredMotorCycleRepository
{
    public async Task<RegisteredMotorcycle?> GetByPlateNumberAsync(string plateNumber, CancellationToken cancellationToken = default)
    {
        return await SetAsNoTracking.FirstOrDefaultAsync(x => x.PlateNumber == plateNumber, cancellationToken);
    }
}