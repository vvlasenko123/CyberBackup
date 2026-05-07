using Domain.Auth;
using Infrastructure.Core.Repositories.Contracts.Create;

namespace Domain.Repositories;

/// <summary>
/// Репозиторий refresh token
/// </summary>
public interface IRefreshTokenRepository : ICreateRepository<RefreshTokenModel, Guid>
{
    /// <summary>
    /// Создать refresh token
    /// </summary>
    Task CreateRefreshTokenAsync(
        RefreshTokenModel refreshTokenModel,
        CancellationToken cancellationToken);
}