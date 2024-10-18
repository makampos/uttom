namespace Uttom.Domain.Interfaces.Abstractions;

public interface IUnitOfWork
{
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}