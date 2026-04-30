using Infrastructure.Core.DDD.ValueObject;
using Infrastructure.Exceptions.User;

namespace Domain.User.ValueObjects;

/// <summary>
/// Пароль пользователя
/// </summary>
public sealed class PasswordHash : ValueObject<PasswordHash>
{
    /// <summary>
    /// Пароль
    /// </summary>
    public string Value { get; }

    public PasswordHash(string value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public override bool Equals(PasswordHash? other)
    {
        return other is not null && Value == other.Value;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}