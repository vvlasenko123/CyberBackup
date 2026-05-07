using Domain.User.Enums;

namespace Application.DTO.Auth;

/// <summary>
/// Результат входа.
/// </summary>
public sealed record LoginResultDto(
    Guid UserId,
    UserRole Role,
    string AccessToken,
    string RefreshToken,
    Guid SessionId,
    DateTimeOffset AccessTokenExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc);