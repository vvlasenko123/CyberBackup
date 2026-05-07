namespace Api.Services.Auth;

/// <summary>
/// Сервис работы с auth-cookie.
/// </summary>
public interface IAuthCookieService
{
    /// <summary>
    /// Установить auth-cookie.
    /// </summary>
    void AppendAuthenticationCookies(
        HttpResponse response,
        string accessToken,
        string refreshToken,
        DateTimeOffset accessTokenExpiresAtUtc,
        DateTimeOffset refreshTokenExpiresAtUtc);

    /// <summary>
    /// Удалить auth-cookie.
    /// </summary>
    void DeleteAuthenticationCookies(HttpResponse response);
}