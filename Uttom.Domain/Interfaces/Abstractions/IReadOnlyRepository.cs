using Uttom.Domain.Models;
using Uttom.Domain.Results;

namespace Uttom.Domain.Interfaces.Abstractions;

public interface IReadOnlyRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<TEntity>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}