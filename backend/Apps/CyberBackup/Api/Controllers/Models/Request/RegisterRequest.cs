namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос регистрации.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// Почта.
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// ФИО.
    /// </summary>
    public string FullName { get; init; }

    /// <summary>
    /// Пароль.
    /// </summary>
    public string Password { get; init; }
}