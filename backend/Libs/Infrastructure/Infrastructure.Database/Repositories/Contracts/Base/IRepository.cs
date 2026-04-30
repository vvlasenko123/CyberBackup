using Infrastructure.Core.DDD.Aggregate.Contract;
using Infrastructure.Database.Repositories.Contracts.Create;
using Infrastructure.Database.Repositories.Contracts.Delete;
using Infrastructure.Database.Repositories.Contracts.Get;
using Infrastructure.Database.Repositories.Contracts.Update;

namespace Infrastructure.Database.Repositories.Contracts.Base;

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