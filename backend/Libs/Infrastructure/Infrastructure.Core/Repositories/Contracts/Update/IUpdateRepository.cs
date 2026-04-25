using Infrastructure.Core.Domain;

namespace Infrastructure.Core.Repositories.Contracts.Update;

/// <summary>
/// Базовый репозиторий обновления
/// </summary>
public interface IUpdateRepository<in TEntity> where TEntity : DomainEntity
{
    /// <summary>
    /// Обновление сущности
    /// </summary>
    Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken);
}