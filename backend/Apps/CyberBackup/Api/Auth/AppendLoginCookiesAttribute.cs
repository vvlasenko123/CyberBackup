using Microsoft.AspNetCore.Mvc;

namespace Api.Auth;

/// <summary>
/// Атрибут добавления auth-cookie после успешного входа.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AppendLoginCookiesAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Создать атрибут добавления auth-cookie.
    /// </summary>
    public AppendLoginCookiesAttribute() : base(typeof(AppendLoginCookiesFilter))
    {
    }
}