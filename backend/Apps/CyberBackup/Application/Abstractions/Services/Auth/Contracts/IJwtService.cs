using Application.DTO.Auth;

namespace Application.Abstractions.Services.Auth.Contracts;

/// <summary>
/// Сервис генерации JWT токена.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Сгенерировать access token.
    /// </summary>
    GeneratedAccessTokenDto GenerateAccessToken(TokenUserDataDto userData);
}