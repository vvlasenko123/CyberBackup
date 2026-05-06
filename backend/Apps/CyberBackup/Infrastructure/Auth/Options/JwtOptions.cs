using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Options;

/// <summary>
/// Настройки JWT.
/// </summary>
public sealed class JwtOptions : IValidateOptions<JwtOptions>
{
    /// <summary>
    /// Издатель токена.
    /// </summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>
    /// Получатель токена.
    /// </summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>
    /// Секретный ключ подписи.
    /// </summary>
    public string SigningKey { get; init; } = string.Empty;

    /// <summary>
    /// Время жизни access token в минутах.
    /// </summary>
    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            return ValidateOptionsResult.Fail("Издатель JWT не настроен.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            return ValidateOptionsResult.Fail("Получатесь JWT не настроен.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            return ValidateOptionsResult.Fail("Ключ подписи JWT не настроен");
        }

        var signingKeyBytes = Encoding.UTF8.GetBytes(options.SigningKey);

        if (signingKeyBytes.Length < 32)
        {
            return ValidateOptionsResult.Fail("Ключ подписи JWT должен содержать минимум 32 байта.");
        }

        if (options.AccessTokenLifetimeMinutes <= 0)
        {
            return ValidateOptionsResult.Fail("Время жизни access token должно быть больше нуля.");
        }

        return ValidateOptionsResult.Success;
    }
}