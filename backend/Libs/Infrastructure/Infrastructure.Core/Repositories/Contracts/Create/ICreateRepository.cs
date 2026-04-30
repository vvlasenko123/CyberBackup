using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Core.Repositories.Contracts.Create;

/// <summary>
/// Базовый репозиторий создания
/// </summary>
public interface ICreateRepository<in TEntity, TType> where TEntity : IAggregateRoot<TType>
{

}