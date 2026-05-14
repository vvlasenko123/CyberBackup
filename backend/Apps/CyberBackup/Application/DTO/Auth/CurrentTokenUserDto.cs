using Domain.User.Enums;

namespace Application.DTO.Auth;

/// <summary>
/// Данные текущего пользователя из токена
/// </summary>
public sealed record CurrentTokenUserDto(
    Guid UserId,
    UserRole Role);