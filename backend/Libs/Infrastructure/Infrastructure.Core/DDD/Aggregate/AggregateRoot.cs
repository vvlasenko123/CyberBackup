using Infrastructure.Core.DDD.Aggregate.Contract;

namespace Infrastructure.Core.DDD.Aggregate;

/// <inheritdoc />
public abstract class AggregateRoot<TType> : IAggregateRoot<TType> 
{
    /// <inheritdoc />
    public TType Id { get; protected set; }

    protected AggregateRoot(TType id)
    {
        Id = id;
    }
}