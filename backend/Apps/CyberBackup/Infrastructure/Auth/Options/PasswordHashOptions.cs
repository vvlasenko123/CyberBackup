using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Options;

/// <summary>
/// Настройки хэширования паролей.
/// </summary>
public sealed class PasswordHashOptions : IValidateOptions<PasswordHashOptions>
{
    /// <summary>
    /// Pepper для предварительного HMAC-хэширования пароля.
    /// </summary>
    public string Pepper { get; init; } = string.Empty;

    /// <summary>
    /// Сложность BCrypt.
    /// </summary>
    public int WorkFactor { get; init; } = 12;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, PasswordHashOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Pepper))
        {
            return ValidateOptionsResult.Fail("Pepper для паролей не настроен.");
        }

        byte[] pepperBytes;

        try
        {
            pepperBytes = Convert.FromBase64String(options.Pepper);
        }
        catch (FormatException)
        {
            return ValidateOptionsResult.Fail("Pepper для паролей должен быть строкой в формате Base64.");
        }

        if (pepperBytes.Length < 32)
        {
            return ValidateOptionsResult.Fail("Pepper для паролей должен содержать минимум 32 байта.");
        }

        if (options.WorkFactor < 10)
        {
            return ValidateOptionsResult.Fail("Сложность BCrypt должна быть не меньше 10.");
        }

        return ValidateOptionsResult.Success;
    }
}