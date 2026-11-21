namespace Domain.User.ValueObjects;

/// <summary>
/// Номер телефона. Хранится как необязательный «+» и цифры
/// </summary>
public readonly record struct PhoneNumber
{
    private const int MaxInputLength = 200;
    private const int MinDigits = 10;
    private const int MaxDigits = 15;

    /// <summary>
    /// Нормализованный номер
    /// </summary>
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Разобрать номер: цифры, пробелы, дефисы, скобки и необязательный ведущий плюс, 10-15 цифр
    /// </summary>
    /// <returns>false, если формат некорректен или строка пустая</returns>
    public static bool TryParse(string? input, out PhoneNumber phone)
    {
        phone = default;

        var text = input?.Trim();

        if (string.IsNullOrEmpty(text) || text.Length > MaxInputLength)
        {
            return false;
        }

        var digits = new List<char>();

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsAsciiDigit(c))
            {
                digits.Add(c);
            }
            else if (!(c == '+' && i == 0) && c is not (' ' or '-' or '(' or ')'))
            {
                return false;
            }
        }

        if (digits.Count is < MinDigits or > MaxDigits)
        {
            return false;
        }

        phone = new PhoneNumber((text[0] == '+' ? "+" : string.Empty) + new string(digits.ToArray()));

        return true;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value;
    }
}
