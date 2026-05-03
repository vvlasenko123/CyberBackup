using Domain.User.Enums;

namespace Application.DTO.Auth;

/// <summary>
/// Результат регистрации.
/// </summary>
public sealed record RegisterResultDto(
    Guid UserId,
    UserRole Role,
    string AccessToken,
    string JwtId,
    Guid SessionId,
    string ClientId,
    IReadOnlyCollection<string> Scopes,
    IReadOnlyCollection<string> Roles,
    DateTimeOffset IssuedAtUtc,
    DateTimeOffset ExpiresAtUtc);