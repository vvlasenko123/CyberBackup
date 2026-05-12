using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос регистрации.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>
    /// Почта.
    /// </summary>
    [Required(ErrorMessage = "Email не должен быть пустым")]
    [EmailAddress(ErrorMessage = "Email имеет некорректный формат")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// ФИО.
    /// </summary>
    [Required(ErrorMessage = "ФИО не должно быть пустым")]
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Пароль.
    /// </summary>
    [Required(ErrorMessage = "Пароль не должен быть пустым")]
    public string Password { get; init; } = string.Empty;
}