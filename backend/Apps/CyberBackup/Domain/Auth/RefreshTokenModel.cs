using Infrastructure.Core.DDD.Aggregate;

namespace Domain.Auth;

/// <summary>
/// Refresh token пользователя.
/// </summary>
public sealed class RefreshTokenModel : AggregateRoot<Guid>
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Хэш refresh token.
    /// </summary>
    public string TokenHash { get; private set; }

    /// <summary>
    /// Идентификатор access token, с которым был создан refresh token.
    /// </summary>
    public string AccessTokenJti { get; private set; }

    /// <summary>
    /// Идентификатор auth-сессии.
    /// </summary>
    public Guid SessionId { get; private set; }

    /// <summary>
    /// Идентификатор клиента.
    /// </summary>
    public string ClientId { get; private set; }

    /// <summary>
    /// Дата создания refresh token.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>
    /// Дата истечения refresh token.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Дата использования refresh token при ротации.
    /// </summary>
    public DateTimeOffset? ConsumedAtUtc { get; private set; }

    /// <summary>
    /// Дата отзыва refresh token.
    /// </summary>
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Идентификатор refresh token, который заменил текущий token.
    /// </summary>
    public Guid? ReplacedByTokenId { get; private set; }

    /// <summary>
    /// IP-адрес, с которого был создан refresh token.
    /// </summary>
    public string? CreatedByIp { get; private set; }

    /// <summary>
    /// Хэш user agent.
    /// </summary>
    public string? UserAgentHash { get; private set; }

    /// <summary>
    /// Создать refresh token.
    /// </summary>
    public RefreshTokenModel(
        Guid id,
        Guid userId,
        string tokenHash,
        string accessTokenJti,
        Guid sessionId,
        string clientId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset? consumedAtUtc,
        DateTimeOffset? revokedAtUtc,
        Guid? replacedByTokenId,
        string? createdByIp,
        string? userAgentHash) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        AccessTokenJti = accessTokenJti;
        SessionId = sessionId;
        ClientId = clientId;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        ConsumedAtUtc = consumedAtUtc;
        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenId = replacedByTokenId;
        CreatedByIp = createdByIp;
        UserAgentHash = userAgentHash;
    }

    /// <summary>
    /// Проверить, активен ли refresh token.
    /// </summary>
    public bool IsActive(DateTimeOffset nowUtc)
    {
        return ConsumedAtUtc is null
               && RevokedAtUtc is null
               && ExpiresAtUtc > nowUtc;
    }
}