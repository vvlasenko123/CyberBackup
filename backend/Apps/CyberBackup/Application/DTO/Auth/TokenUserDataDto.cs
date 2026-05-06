namespace Application.DTO.Auth;

/// <summary>
/// Данные пользователя для генерации токена.
/// </summary>
public sealed record TokenUserDataDto(
    Guid SubjectId,
    string ClientId,
    Guid SessionId,
    IReadOnlyCollection<string> Scopes,
    IReadOnlyCollection<string> Roles);