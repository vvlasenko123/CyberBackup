namespace Infrastructure.Auth.Options;

/// <summary>
/// Настройки JWT.
/// </summary>
public sealed class JwtOptions
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
}