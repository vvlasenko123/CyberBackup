using Domain.User.Enums;

namespace Application.DTO;

/// <summary>
/// user dto
/// </summary>
public sealed record UserDto
{
    /// <summary>
    /// Айди текущего пользователя
    /// </summary>
    public Guid CurrentUserId { get; private set; }

    /// <summary>
    /// Почта
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; private set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; private set; }
}