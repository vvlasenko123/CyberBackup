using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Abstractions.Services.Auth.Contracts;
using Application.DTO.Auth;
using Infrastructure.Auth.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private const string AccessTokenType = "at+jwt";

    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        ValidateOptions(_options);

        var signingKeyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        var securityKey = new SymmetricSecurityKey(signingKeyBytes);

        _signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);
    }

    public GeneratedAccessTokenDto GenerateAccessToken(TokenUserDataDto userData)
    {
        ArgumentNullException.ThrowIfNull(userData);

        var issuedAtUtc = DateTimeOffset.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var jwtId = Guid.CreateVersion7().ToString("N");

        var claims = BuildClaims(userData, issuedAtUtc, jwtId);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = issuedAtUtc.UtcDateTime,
            IssuedAt = issuedAtUtc.UtcDateTime,
            Expires = expiresAtUtc.UtcDateTime,
            SigningCredentials = _signingCredentials,
            TokenType = AccessTokenType
        };

        var token = _tokenHandler.CreateToken(descriptor);
        var accessToken = _tokenHandler.WriteToken(token);

        return new GeneratedAccessTokenDto(
            AccessToken: accessToken,
            JwtId: jwtId,
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: expiresAtUtc);
    }

    private static List<Claim> BuildClaims(
        TokenUserDataDto userData,
        DateTimeOffset issuedAtUtc,
        string jwtId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userData.SubjectId),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(
                JwtRegisteredClaimNames.Iat,
                issuedAtUtc.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new("client_id", userData.ClientId),
            new("sid", userData.SessionId.ToString("D"))
        };

        var scopes = userData.Scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (scopes.Length > 0)
        {
            claims.Add(new Claim("scope", string.Join(' ', scopes)));
        }

        var roles = userData.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        return claims;
    }

    private static void ValidateOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var signingKeyBytes = Encoding.UTF8.GetBytes(options.SigningKey);

        if (signingKeyBytes.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must be at least 32 bytes long.");
        }

        if (options.AccessTokenLifetimeMinutes <= 0)
        {
            throw new InvalidOperationException("JWT access token lifetime must be greater than zero.");
        }
    }
}