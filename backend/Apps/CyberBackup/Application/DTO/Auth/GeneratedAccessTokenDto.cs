namespace Application.DTO.Auth;

public sealed record GeneratedAccessTokenDto(
    string AccessToken,
    string JwtId,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc);