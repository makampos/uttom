using Uttom.Domain.Interfaces;
using Uttom.Domain.Models;

namespace Uttom.Infrastructure.Implementations;

public class Repository<TEntity>(
    ApplicationDbContext applicationDbContext
    ) : ReadOnlyRepository<TEntity>(applicationDbContext),
    IRepository<TEntity> where TEntity : Entity
{
    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is TrackableEntity trackable)
        {
            trackable.Created("admin");
        }

        await Set.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is TrackableEntity trackable)
        {
            trackable.Updated("admin");
        }

        await Task.Run(() =>
        {
            Set.Update(entity);
        }, cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is TrackableEntity trackable)
        {
            trackable.Deleted("admin");

            await Task.Run(() =>
            {
                Set.Update(entity);
            }, cancellationToken);
        }
        else
        {
            await Task.Run(() =>
            {
                Set.Remove(entity);
            }, cancellationToken);
        }
    }
}