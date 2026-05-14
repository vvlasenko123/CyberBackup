using System.Text.RegularExpressions;
using Infrastructure.Core.DDD.ValueObject;
using Infrastructure.Exceptions.User;

namespace Domain.User.ValueObjects;

/// <summary>
/// ФИО пользователя
/// </summary>
public sealed class FullName : ValueObject<FullName>
{
    /// <summary>
    /// ФИО
    /// </summary>
    public string Value { get; }
    
    /// <summary>
    /// Допустимые символы в ФИО
    /// </summary>
    private static readonly Regex _regex = new("^[a-zA-Zа-яА-ЯёЁ\\s-]+$", RegexOptions.Compiled);

    public FullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidFullNameException("ФИО не должно быть пустым");
        }

        fullName = fullName.Trim();

        if (fullName.Length > 255)
        {
            throw new InvalidFullNameException("ФИО слишком длинное");
        }

        if (!_regex.IsMatch(fullName))
        {
            throw new InvalidFullNameException("ФИО содержит недопустимые символы");
        }

        Value = fullName;
    }

    /// <inheritdoc />
    public override bool Equals(FullName? other)
    {
        return other is not null && Value == other.Value;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}