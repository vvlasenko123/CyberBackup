using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос входа.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>
    /// Почта.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Пароль.
    /// </summary>
    [Required]
    public string Password { get; init; } = string.Empty;
}