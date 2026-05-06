using Application.DTO.Auth;

namespace Application.Abstractions.Services.Auth.Contracts;

/// <summary>
/// Сервис работы с refresh token.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Сгенерировать refresh token.
    /// </summary>
    /// <param name="createdAtUtc">Дата создания refresh token.</param>
    /// <returns>Данные сгенерированного refresh token.</returns>
    GeneratedRefreshTokenDto GenerateRefreshToken(DateTimeOffset createdAtUtc);

    /// <summary>
    /// Получить хэш refresh token.
    /// </summary>
    /// <param name="refreshToken">Открытое значение refresh token.</param>
    /// <returns>Хэш refresh token.</returns>
    string Hash(string refreshToken);
}