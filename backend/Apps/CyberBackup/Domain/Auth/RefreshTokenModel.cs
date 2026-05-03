using Infrastructure.Core.DDD.Aggregate;

namespace Domain.Auth;

public sealed class RefreshTokenModel : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public string AccessTokenJti { get; private set; }

    public Guid SessionId { get; private set; }

    public string ClientId { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? ConsumedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? UserAgentHash { get; private set; }

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

    public bool IsActive(DateTimeOffset nowUtc)
    {
        return ConsumedAtUtc is null
               && RevokedAtUtc is null
               && ExpiresAtUtc > nowUtc;
    }
}