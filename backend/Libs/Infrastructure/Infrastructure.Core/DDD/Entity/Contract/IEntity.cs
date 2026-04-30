using Infrastructure.Core.DDD.BaseContracts;

namespace Infrastructure.Core.DDD.Entity.Contract;

/// <summary>
/// Entity
/// </summary>
public interface IEntity<out TType> : IIdentity<TType>
{
}