using Infrastructure.Core.DDD.Aggregate.Contract;
using Infrastructure.Core.Repositories.Contracts.Create;
using Infrastructure.Core.Repositories.Contracts.Delete;
using Infrastructure.Core.Repositories.Contracts.Get;
using Infrastructure.Core.Repositories.Contracts.Update;

namespace Infrastructure.Core.Repositories.Contracts.Base;

/// <summary>
/// Базовый репозиторий
/// </summary>
public interface IRepository<TEntity, TType> :
    ICreateRepository<TEntity, TType>,
    IGetRepository<TEntity, TType>,
    IDeleteRepository<TEntity, TType>,
    IUpdateRepository<TEntity, TType>
    where TEntity : IAggregateRoot<TType>
{
}