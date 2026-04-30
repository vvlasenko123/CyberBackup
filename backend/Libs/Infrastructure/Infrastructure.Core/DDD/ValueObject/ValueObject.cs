using Infrastructure.Core.DDD.ValueObject.Contracts;

namespace Infrastructure.Core.DDD.ValueObject;

/// <inheritdoc />
public abstract class ValueObject<TType> : IValueObject<TType> 
    where TType : ValueObject<TType>
{
    /// <inheritdoc />
    public abstract bool Equals(TType? other);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is not TType other)
        {
            return false;
        }

        return Equals(other);
    }

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <summary>
    /// Переопределение оператора равенства для VO
    /// </summary>
    public static bool operator ==(ValueObject<TType>? left, ValueObject<TType>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals((TType)right);
    }

    /// <summary>
    /// Переопределение оператора не равенства для VO
    /// </summary>
    public static bool operator !=(ValueObject<TType>? left, ValueObject<TType>? right)
    {
        return !(left == right);
    }
}