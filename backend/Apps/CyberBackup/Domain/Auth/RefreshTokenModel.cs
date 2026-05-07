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
    /// Идентификатор auth-сессии.
    /// </summary>
    public Guid SessionId { get; private set; }

    /// <summary>
    /// Дата создания refresh token.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; private set; }

    /// <summary>
    /// Дата истечения refresh token.
    /// </summary>
    public DateTimeOffset ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Дата отзыва refresh token.
    /// </summary>
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Идентификатор refresh token, который заменил текущий token.
    /// </summary>
    public Guid? ReplacedByTokenId { get; private set; }

    /// <summary>
    /// Создать refresh token.
    /// </summary>
    public RefreshTokenModel(
        Guid id,
        Guid userId,
        string tokenHash,
        Guid sessionId,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc,
        DateTimeOffset? revokedAtUtc,
        Guid? replacedByTokenId) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        SessionId = sessionId;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenId = replacedByTokenId;
    }

    /// <summary>
    /// Проверить, активен ли refresh token.
    /// </summary>
    public bool IsActive(DateTimeOffset nowUtc)
    {
        return RevokedAtUtc is null
               && ExpiresAtUtc > nowUtc;
    }
}