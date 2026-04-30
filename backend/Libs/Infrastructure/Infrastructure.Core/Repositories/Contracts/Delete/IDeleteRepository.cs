using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Core.Repositories.Contracts.Delete;

/// <summary>
/// Базовый репозиторий удаления
/// </summary>
public interface IDeleteRepository<in TEntity, TType> where TEntity : IAggregateRoot<TType>
{
}