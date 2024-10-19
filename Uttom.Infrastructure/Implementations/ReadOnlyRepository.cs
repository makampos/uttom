using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Infrastructure.Implementations;

public class ReadOnlyRepository<TEntity>(
    ApplicationDbContext applicationDbContext
) : RepositoryProperties<TEntity>(applicationDbContext), IReadOnlyRepository<TEntity> where TEntity : Entity
{
    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await SetAsTracking.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<PagedResult<TEntity>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var totalCount = await SetAsNoTracking.CountAsync(cancellationToken);
        var items = await SetAsNoTracking
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return  new PagedResult<TEntity>(items, totalCount, pageSize, pageNumber);
    }
}