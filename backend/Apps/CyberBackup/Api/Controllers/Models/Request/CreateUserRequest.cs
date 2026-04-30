using Domain.User.Enums;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос на создание пользователя
/// </summary>
public sealed record CreateUserRequest
{
    /// <summary>
    /// Почта
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FullName { get; init; }

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; init; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; init; }
}