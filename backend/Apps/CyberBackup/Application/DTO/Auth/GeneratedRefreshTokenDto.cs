namespace Application.DTO.Auth;


public sealed record GeneratedRefreshTokenDto(
    string RefreshToken,
    string RefreshTokenHash,
    DateTimeOffset ExpiresAtUtc);