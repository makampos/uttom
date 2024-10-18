using Uttom.Domain.Interfaces.Repositories;
using Uttom.Domain.Messages;
using Uttom.Infrastructure.Implementations;

namespace Uttom.Infrastructure.Repositories;

public class RegisteredMotorCycleRepository(ApplicationDbContext applicationDbContext)
    : Repository<RegisteredMotorcycle>(applicationDbContext), IRegisteredMotorCycleRepository
{

}