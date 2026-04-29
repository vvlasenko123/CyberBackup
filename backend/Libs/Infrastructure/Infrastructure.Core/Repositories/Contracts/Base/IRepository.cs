using Infrastructure.Core.Domain;
using Infrastructure.Core.Repositories.Contracts.Create;
using Infrastructure.Core.Repositories.Contracts.Delete;
using Infrastructure.Core.Repositories.Contracts.Get;
using Infrastructure.Core.Repositories.Contracts.Update;

namespace Infrastructure.Core.Repositories.Contracts.Base;

/// <summary>
/// Базовый репозиторий
/// </summary>
public interface IRepository<TEntity> : 
    ICreateRepository<TEntity>,
    IDeleteRepository<TEntity>,
    IGetRepository<TEntity>,
    IUpdateRepository<TEntity> 
    where TEntity : DomainEntity
{
}