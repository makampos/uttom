using Uttom.Domain.Interfaces.Abstractions;
using Uttom.Domain.Models;

namespace Uttom.Domain.Interfaces;

public interface IRepository<TEntity> : IReadOnlyRepository<TEntity> where TEntity : Entity
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}