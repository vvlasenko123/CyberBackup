namespace Application.DTO.Auth;

/// <summary>
/// Результат входа.
/// </summary>
public sealed record LoginResultDto(
    Guid UserId,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    DateTimeOffset RefreshTokenExpiresAtUtc);