using Application.DTO.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Auth;

/// <summary>
/// Фильтр добавления auth-cookie после успешного входа.
/// </summary>
public sealed class AppendLoginCookiesFilter : IAsyncResultFilter
{
    private static readonly CookieBuilder AuthCookieBuilder = new()
    {
        HttpOnly = true,
        SecurePolicy = CookieSecurePolicy.Always,
        SameSite = SameSiteMode.None,
        IsEssential = true,
        Path = "/"
    };

    private readonly ICookieManager _cookieManager;

    public AppendLoginCookiesFilter(ICookieManager cookieManager)
    {
        _cookieManager = cookieManager;
    }

    /// <inheritdoc />
    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        if (context.Result is not ObjectResult { Value: LoginResultDto result })
        {
            await next();
            return;
        }

        _cookieManager.AppendResponseCookie(
            context.HttpContext,
            AuthCookieNames.AccessToken,
            result.AccessToken,
            CreateCookieOptions(context.HttpContext, result.AccessTokenExpiresAtUtc));

        _cookieManager.AppendResponseCookie(
            context.HttpContext,
            AuthCookieNames.RefreshToken,
            result.RefreshToken,
            CreateCookieOptions(context.HttpContext, result.RefreshTokenExpiresAtUtc));

        context.Result = new OkResult();

        await next();
    }

    /// <summary>
    /// Создать настройки auth-cookie.
    /// </summary>
    private static CookieOptions CreateCookieOptions(
        HttpContext httpContext,
        DateTimeOffset expiresAtUtc)
    {
        var options = AuthCookieBuilder.Build(httpContext);
        options.Expires = expiresAtUtc;

        return options;
    }
}