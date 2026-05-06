using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services.Auth.Contracts;
using Application.DTO.Auth;
using Infrastructure.Auth.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Services;

public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly RefreshTokenOptions _options;

    public RefreshTokenService(IOptions<RefreshTokenOptions> options)
    {
        _options = options.Value;
        ValidateOptions(_options);
    }

    public GeneratedRefreshTokenDto GenerateRefreshToken(DateTimeOffset createdAtUtc)
    {
        var bytes = RandomNumberGenerator.GetBytes(_options.TokenBytes);
        var refreshToken = ToBase64Url(bytes);
        var refreshTokenHash = Hash(refreshToken);
        var expiresAtUtc = createdAtUtc.AddDays(_options.LifetimeDays);

        return new GeneratedRefreshTokenDto(
            RefreshToken: refreshToken,
            RefreshTokenHash: refreshTokenHash,
            ExpiresAtUtc: expiresAtUtc);
    }

    public string Hash(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new InvalidOperationException("Refresh token must not be empty.");
        }

        var bytes = Encoding.UTF8.GetBytes(refreshToken);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

    private static string ToBase64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static void ValidateOptions(RefreshTokenOptions options)
    {
        if (options.LifetimeDays <= 0)
        {
            throw new InvalidOperationException("Refresh token lifetime must be greater than zero.");
        }

        if (options.TokenBytes < 32)
        {
            throw new InvalidOperationException("Refresh token must contain at least 32 random bytes.");
        }
    }
}