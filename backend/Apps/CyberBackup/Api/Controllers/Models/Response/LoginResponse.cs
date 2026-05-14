namespace Api.Controllers.Models.Response;

/// <summary>
/// Ответ входа
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt);