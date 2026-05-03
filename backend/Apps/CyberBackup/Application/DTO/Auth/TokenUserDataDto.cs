namespace Application.DTO.Auth;

public sealed record TokenUserDataDto(
    string SubjectId,
    string ClientId,
    Guid SessionId,
    IReadOnlyCollection<string> Scopes,
    IReadOnlyCollection<string> Roles);