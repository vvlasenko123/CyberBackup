using Domain.Auth;
using Domain.Repositories;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IAsyncDbConnection _connection;

    public RefreshTokenRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task CreateRefreshTokenAsync(
        RefreshTokenModel refreshTokenModel,
        CancellationToken cancellationToken)
    {
        const string sql = """
                               INSERT INTO refresh_tokens (
                                   id,
                                   user_id,
                                   token_hash,
                                   session_id,
                                   created_at_utc,
                                   expires_at_utc,
                                   revoked_at_utc,
                                   replaced_by_token_id
                               )
                               VALUES (
                                   @Id,
                                   @UserId,
                                   @TokenHash,
                                   @SessionId,
                                   @CreatedAtUtc,
                                   @ExpiresAtUtc,
                                   @RevokedAtUtc,
                                   @ReplacedByTokenId
                               )
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            refreshTokenModel.Id,
            refreshTokenModel.UserId,
            refreshTokenModel.TokenHash,
            refreshTokenModel.SessionId,
            refreshTokenModel.CreatedAtUtc,
            refreshTokenModel.ExpiresAtUtc,
            refreshTokenModel.RevokedAtUtc,
            refreshTokenModel.ReplacedByTokenId
        }, cancellationToken);
    }
}