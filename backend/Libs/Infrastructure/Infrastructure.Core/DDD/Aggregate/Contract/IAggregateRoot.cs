using Infrastructure.Core.DDD.Entity.Contract;

namespace Infrastructure.Core.DDD.Aggregate.Contract;

/// <summary>
/// Aggregate root
/// </summary>
public interface IAggregateRoot<out TType> : IEntity<TType>
{
}