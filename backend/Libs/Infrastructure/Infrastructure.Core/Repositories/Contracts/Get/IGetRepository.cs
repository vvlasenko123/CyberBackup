using Infrastructure.Core.Domain;

namespace Infrastructure.Core.Repositories.Contracts.Get;

/// <summary>
/// Базовый репозиторий получения
/// </summary>
public interface IGetRepository<TEntity> where TEntity : DomainEntity
{
    /// <summary>
    /// Получение сущности по идентификатору
    /// </summary>
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Получение списка сущностей
    /// </summary>
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
}