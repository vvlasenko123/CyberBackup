using System.ComponentModel.DataAnnotations;
using Domain.User.Enums;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос на изменение пользователя
/// </summary>
public sealed record UpdateUserRequest
{
    /// <summary>
    /// Почта
    /// </summary>
    [Required(ErrorMessage = "Email не должен быть пустым")]
    [EmailAddress(ErrorMessage = "Email имеет некорректный формат")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Имя пользователя
    /// </summary>
    [Required(ErrorMessage = "ФИО не должно быть пустым")]
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    [Required(ErrorMessage = "Роль не должна быть пустой")]
    public UserRole Role { get; init; }

    /// <summary>
    /// Активен ли пользователь.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Нужно ли сменить пароль.
    /// </summary>
    public bool MustChangePassword { get; init; }
}