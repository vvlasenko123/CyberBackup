using Domain.User.Enums;

namespace Application.DTO.User;

/// <summary>
/// DTO изменения пользователя
/// </summary>
public sealed record UpdateUserDto
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public Guid Id { get; init; }

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
    public string? Password { get; init; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; init; }

    /// <summary>
    /// Активен ли пользователь
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Нужно ли сменить пароль
    /// </summary>
    public bool MustChangePassword { get; init; }
}