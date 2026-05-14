using System.ComponentModel.DataAnnotations;
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
    [Required(ErrorMessage = "Пароль не должен быть пустым")]
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Роль пользователя
    /// </summary>
    [Required(ErrorMessage = "Роль не должна быть пустой")]
    public UserRole Role { get; init; }
}