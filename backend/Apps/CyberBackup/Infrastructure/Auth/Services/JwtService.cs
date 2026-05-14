using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Abstractions.Services.Auth.Contracts;
using Application.DTO.Auth;
using Domain.User.Enums;
using Infrastructure.Auth.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Auth.Services;

/// <summary>
/// Сервис генерации JWT.
/// </summary>
public sealed class JwtService : IJwtService
{
    private const string AccessTokenType = "at+jwt";

    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SigningCredentials _signingCredentials;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Создать сервис генерации JWT.
    /// </summary>
    public JwtService(IOptions<JwtOptions> options, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _options = options.Value;
        _tokenHandler = new JwtSecurityTokenHandler();

        var signingKeyBytes = Encoding.UTF8.GetBytes(_options.SigningKey);
        var securityKey = new SymmetricSecurityKey(signingKeyBytes);

        _signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);
    }

    /// <inheritdoc />
    public GeneratedAccessTokenDto GenerateAccessToken(TokenUserDataDto userData)
    {
        ArgumentNullException.ThrowIfNull(userData);

        var issuedAtUtc = DateTimeOffset.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var jwtId = UUIDNext.Uuid.NewSequential().ToString("N");

        var claims = BuildCustomClaims(userData, issuedAtUtc, jwtId);

        var descriptor = new SecurityTokenDescriptor
        {
            // iss, aud, nbf, exp добавляются в payload через descriptor.
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

    /// <inheritdoc />
    public CurrentTokenUserDto GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user is null)
        {
            throw new InvalidOperationException("Контекст пользователя отсутствует");
        }

        var userIdText = user.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userIdText))
        {
            throw new InvalidOperationException("В токене отсутствует идентификатор пользователя");
        }

        if (!Guid.TryParse(userIdText, out var userId))
        {
            throw new InvalidOperationException("В токене некорректный идентификатор пользователя");
        }
        
        var roleText = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue(AuthClaimNames.Role);

        if (string.IsNullOrWhiteSpace(roleText))
        {
            throw new InvalidOperationException("В токене отсутствует роль пользователя");
        }

        if (!Enum.TryParse<UserRole>(roleText, true, out var role))
        {
            throw new InvalidOperationException("В токене некорректная роль пользователя");
        }

        return new CurrentTokenUserDto(UserId: userId, Role: role);
    }

    /// <summary>
    /// Собрать custom claims для access token.
    /// </summary>
    private static List<Claim> BuildCustomClaims(
        TokenUserDataDto userData,
        DateTimeOffset issuedAtUtc,
        string jwtId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userData.SubjectId.ToString("D")),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(
                JwtRegisteredClaimNames.Iat,
                issuedAtUtc.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),
            new(AuthClaimNames.ClientId, userData.ClientId),
            new(AuthClaimNames.SessionId, userData.SessionId.ToString("D"))
        };

        var scopeSet = new HashSet<string>(StringComparer.Ordinal);

        foreach (var scope in userData.Scopes)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                continue;
            }

            scopeSet.Add(scope);
        }

        if (scopeSet.Count > 0)
        {
            claims.Add(new Claim(AuthClaimNames.Scope, string.Join(' ', scopeSet)));
        }

        var roleSet = new HashSet<string>(StringComparer.Ordinal);

        foreach (var role in userData.Roles)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                continue;
            }

            if (roleSet.Add(role))
            {
                claims.Add(new Claim(AuthClaimNames.Role, role.ToLowerInvariant()));
            }
        }

        return claims;
    }
}