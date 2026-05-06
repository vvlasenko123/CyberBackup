namespace Application.DTO.Auth;

/// <summary>
/// Данные сгенерированного refresh token.
/// </summary>
public sealed record GeneratedRefreshTokenDto(
    string RefreshToken,
    string RefreshTokenHash,
    DateTimeOffset ExpiresAtUtc);