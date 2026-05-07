using Api.Auth;

namespace Api.Services.Auth;

/// <summary>
/// Сервис работы с auth-cookie.
/// </summary>
public sealed class AuthCookieService : IAuthCookieService
{
    /// <inheritdoc />
    public void AppendAuthenticationCookies(
        HttpResponse response,
        string accessToken,
        string refreshToken,
        DateTimeOffset accessTokenExpiresAtUtc,
        DateTimeOffset refreshTokenExpiresAtUtc)
    {
        response.Cookies.Append(
            AuthCookieNames.AccessToken,
            accessToken,
            CreateCookieOptions(accessTokenExpiresAtUtc));

        response.Cookies.Append(
            AuthCookieNames.RefreshToken,
            refreshToken,
            CreateCookieOptions(refreshTokenExpiresAtUtc));
    }

    /// <inheritdoc />
    public void DeleteAuthenticationCookies(HttpResponse response)
    {
        response.Cookies.Delete(AuthCookieNames.AccessToken, CreateDeleteCookieOptions());
        response.Cookies.Delete(AuthCookieNames.RefreshToken, CreateDeleteCookieOptions());
    }

    /// <summary>
    /// Создать настройки auth-cookie.
    /// </summary>
    private static CookieOptions CreateCookieOptions(DateTimeOffset expiresAtUtc)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAtUtc,
            Path = "/"
        };
    }

    /// <summary>
    /// Создать настройки удаления auth-cookie.
    /// </summary>
    private static CookieOptions CreateDeleteCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };
    }
}