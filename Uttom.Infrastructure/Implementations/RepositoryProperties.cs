using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Models;

namespace Uttom.Infrastructure.Implementations;

public class RepositoryProperties<TEntity>(
    ApplicationDbContext applicationDbContext
) where TEntity : Entity
{
    protected readonly ApplicationDbContext ApplicationDbContext = applicationDbContext;

    protected DbSet<TEntity> Set => ApplicationDbContext.Set<TEntity>();

    protected IQueryable<TEntity> SetAsTracking
    {
        get
        {
            var query = Set.AsTracking();

            if (typeof(TEntity).IsSubclassOf(typeof(TrackableEntity)))
            {
                query = query.Where(e => !(e as TrackableEntity)!.IsDeleted);
            }

            return query;
        }
    }

    // Only for read-only operations to improve performance
    protected IQueryable<TEntity> SetAsNoTracking
    {
        get
        {
            var query = Set.AsNoTracking();

            if (typeof(TEntity).IsSubclassOf(typeof(TrackableEntity)))
            {
                query = query.Where(e => !(e as TrackableEntity)!.IsDeleted); // only return active data
            }

            return query;
        }
    }
}