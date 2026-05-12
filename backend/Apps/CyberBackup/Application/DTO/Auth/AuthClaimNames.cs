namespace Application.DTO.Auth;

/// <summary>
/// Имена claims авторизации.
/// </summary>
public static class AuthClaimNames
{
    /// <summary>
    /// Идентификатор клиента.
    /// </summary>
    public const string ClientId = "client_id";

    /// <summary>
    /// Идентификатор auth-сессии.
    /// </summary>
    public const string SessionId = "sid";

    /// <summary>
    /// Scope.
    /// </summary>
    public const string Scope = "scope";

    /// <summary>
    /// Роль.
    /// </summary>
    public const string Role = "role";
}