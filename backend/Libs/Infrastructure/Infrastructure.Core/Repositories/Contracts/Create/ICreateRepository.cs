using Infrastructure.Core.Domain;

namespace Infrastructure.Core.Repositories.Contracts.Create;

/// <summary>
/// Базовый репозиторий создания
/// </summary>
public interface ICreateRepository<in TEntity> where TEntity : DomainEntity
{
    /// <summary>
    /// Добавление сущности
    /// </summary>
    Task<Guid> CreateAsync(TEntity entity, CancellationToken cancellationToken);
}