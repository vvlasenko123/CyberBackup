namespace Application.DTO.Auth;

/// <summary>
/// DTO смены пароля
/// </summary>
public sealed record ChangePasswordDto
{
    /// <summary>
    /// Текущий пароль
    /// </summary>
    public string CurrentPassword { get; init; } = string.Empty;

    /// <summary>
    /// Новый пароль
    /// </summary>
    public string NewPassword { get; init; } = string.Empty;
}