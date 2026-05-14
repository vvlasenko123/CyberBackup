using Application.DTO.Auth;

namespace Application.Abstractions.Services.Auth.Contracts;

/// <summary>
/// Сервис получения текущего пользователя из токена
/// </summary>
public interface ICurrentTokenUserService
{
    /// <summary>
    /// Получить текущего пользователя из токена
    /// </summary>
    CurrentTokenUserDto GetCurrentUser();
}