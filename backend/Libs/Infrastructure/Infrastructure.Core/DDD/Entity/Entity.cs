using Infrastructure.Core.DDD.Entity.Contract;

namespace Infrastructure.Core.DDD.Entity;

/// <inheritdoc />
public abstract class Entity<TType> : IEntity<TType> 
{
    /// <inheritdoc />
    public TType Id { get; protected set; }

    protected Entity(TType id)
    {
        Id = id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TType> other)
        {
            return false;
        }

        return EqualityComparer<TType>.Default.Equals(Id, other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return EqualityComparer<TType>.Default.GetHashCode(Id);
    }
}