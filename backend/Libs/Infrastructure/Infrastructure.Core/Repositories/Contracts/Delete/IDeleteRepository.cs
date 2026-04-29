using Infrastructure.Core.Domain;

namespace Infrastructure.Core.Repositories.Contracts.Delete;

/// <summary>
/// Базовый репозиторий удаления
/// </summary>
public interface IDeleteRepository<TEntity> where TEntity : DomainEntity
{
    /// <summary>
    /// Удаление сущности
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}