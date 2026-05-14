using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос смены пароля
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>
    /// Текущий пароль
    /// </summary>
    [Required(ErrorMessage = "Текущий пароль не должен быть пустым")]
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    [Required(ErrorMessage = "Новый пароль не должен быть пустым")]
    public string NewPassword { get; init; } = string.Empty;
}