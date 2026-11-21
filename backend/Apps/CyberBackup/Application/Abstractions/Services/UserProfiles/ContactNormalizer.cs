using System.Text.RegularExpressions;
using Domain.User.ValueObjects;

namespace Application.Abstractions.Services.UserProfiles;

/// <summary>
/// Проверка и нормализация контактов
/// </summary>
public static partial class ContactNormalizer
{
    private const int MaxInputLength = 200;

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_]{4,31}$")]
    private static partial Regex TelegramPattern();

    [GeneratedRegex(@"^[A-Za-z0-9_.]{5,32}$")]
    private static partial Regex VkPattern();

    [GeneratedRegex(@"^(https?://)?(www\.)?(t\.me|telegram\.me)/", RegexOptions.IgnoreCase)]
    private static partial Regex TelegramUrlPrefix();

    [GeneratedRegex(@"^(https?://)?(www\.|m\.)?vk\.com/", RegexOptions.IgnoreCase)]
    private static partial Regex VkUrlPrefix();

    /// <summary>
    /// Телефон: см. <see cref="PhoneNumber.TryParse"/>. Пустая строка очищает значение
    /// </summary>
    public static PhoneNumber? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return PhoneNumber.TryParse(value, out var phone) ? phone : throw PhoneError();
    }

    /// <summary>
    /// Telegram: имя пользователя 5-32 символа (латиница, цифры, подчёркивание, начинается с буквы).
    /// Допускаются @ и ссылка t.me; хранится без них
    /// </summary>
    public static string? NormalizeTelegram(string? value)
    {
        var input = value?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        if (input.Length > MaxInputLength)
        {
            throw TelegramError();
        }

        input = TelegramUrlPrefix().Replace(input, string.Empty).TrimEnd('/');

        if (input.StartsWith('@'))
        {
            input = input[1..];
        }

        if (!TelegramPattern().IsMatch(input))
        {
            throw TelegramError();
        }

        return input;
    }

    /// <summary>
    /// VK: короткое имя или идентификатор страницы 5-32 символа (латиница, цифры, подчёркивание, точка).
    /// Допускается ссылка vk.com; хранится без неё
    /// </summary>
    public static string? NormalizeVk(string? value)
    {
        var input = value?.Trim();

        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        if (input.Length > MaxInputLength)
        {
            throw VkError();
        }

        input = VkUrlPrefix().Replace(input, string.Empty).TrimEnd('/');

        if (!VkPattern().IsMatch(input))
        {
            throw VkError();
        }

        return input;
    }

    private static ProfileException PhoneError() => new(
        ProfileException.PhoneInvalid,
        "Телефон должен содержать от 10 до 15 цифр; допустимы пробелы, дефисы, скобки и ведущий +");

    private static ProfileException TelegramError() => new(
        ProfileException.TelegramInvalid,
        "Telegram: от 5 до 32 символов, латиница, цифры и _, первая буква обязательна");

    private static ProfileException VkError() => new(
        ProfileException.VkInvalid,
        "VK: от 5 до 32 символов, латиница, цифры, _ и точка");
}
