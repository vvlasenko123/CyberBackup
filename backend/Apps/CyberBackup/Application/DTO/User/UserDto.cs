using Domain.User.Enums;

namespace Application.DTO.User;

/// <summary>
/// user dto
/// </summary>
public sealed record UserDto
{
    /// <summary>
    /// Почта
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Роль создаваемого пользователя
    /// </summary>
    public UserRole Role { get; init; }
}