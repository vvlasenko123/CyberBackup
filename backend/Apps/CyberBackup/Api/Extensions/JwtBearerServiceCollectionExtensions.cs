using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Api.Auth;
using Application.DTO.Auth;
using Infrastructure.Auth.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Extensions;

/// <summary>
/// Методы регистрации JWT-аутентификации.
/// </summary>
public static class JwtBearerServiceCollectionExtensions
{
    /// <summary>
    /// Добавить JWT-аутентификацию через access token из cookie.
    /// </summary>
    public static void AddCyberJwtAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<ICookieManager, ChunkingCookieManager>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>, ICookieManager>(ConfigureJwtBearerOptions);

        services.AddAuthorization();
    }

    private static void ConfigureJwtBearerOptions(
        JwtBearerOptions options,
        IOptions<JwtOptions> jwtOptionsAccessor,
        ICookieManager cookieManager)
    {
        var jwtOptions = jwtOptionsAccessor.Value;
        var signingKeyBytes = Encoding.UTF8.GetBytes(jwtOptions.SigningKey);

        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,

            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = AuthClaimNames.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = cookieManager.GetRequestCookie(
                    context.HttpContext,
                    AuthCookieNames.AccessToken);

                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    }
}