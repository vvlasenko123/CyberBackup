using Domain.User.Enums;

namespace Application.DTO.Auth;

/// <summary>
/// Результат регистрации.
/// </summary>
public sealed record RegisterResultDto(
    Guid UserId,
    UserRole Role,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);