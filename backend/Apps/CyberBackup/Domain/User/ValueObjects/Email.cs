using System.ComponentModel.DataAnnotations;
using Infrastructure.Core.DDD.ValueObject;
using Infrastructure.Exceptions.User;

namespace Domain.User.ValueObjects;

/// <summary>
/// Почта пользователя
/// </summary>
public sealed class Email : ValueObject<Email>
{
    /// <summary>
    /// Почта
    /// </summary>
    public string Value { get; }

    public Email([EmailAddress] string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidEmailException("Email не должен быть пустым");
        }

        Value = value;
    }

    /// <inheritdoc />
    public override bool Equals(Email? other)
    {
        return other is not null && Value == other.Value;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}