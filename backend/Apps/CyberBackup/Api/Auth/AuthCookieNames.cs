namespace Api.Auth;

/// <summary>
/// Имена auth-cookie.
/// </summary>
internal static class AuthCookieNames
{
    /// <summary>
    /// Cookie access token.
    /// </summary>
    public const string AccessToken = "access_token";

    /// <summary>
    /// Cookie refresh token.
    /// </summary>
    public const string RefreshToken = "refresh_token";
}