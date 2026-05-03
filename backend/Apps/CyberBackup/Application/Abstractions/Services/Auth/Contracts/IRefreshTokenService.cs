using Application.DTO.Auth;

namespace Application.Abstractions.Services.Auth.Contracts;

public interface IRefreshTokenService
{
    GeneratedRefreshTokenDto GenerateRefreshToken(DateTimeOffset createdAtUtc);

    string Hash(string refreshToken);
}