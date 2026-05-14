using Domain.Auth;

namespace Domain.Repositories;

/// <summary>
/// Репозиторий refresh token
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>
    /// Создать refresh token
    /// </summary>
    Task CreateRefreshTokenAsync(
        RefreshTokenModel refreshTokenModel,
        CancellationToken cancellationToken);
}