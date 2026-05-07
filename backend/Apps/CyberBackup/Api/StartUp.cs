using System.Reflection;
using Application;
using Infrastructure;
using Infrastructure.AutoMapper;
using Infrastructure.Core;
using Infrastructure.Core.Controllers;
using Infrastructure.Database;
using Infrastructure.Swagger;
using Security.Host.Cors;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Api.Auth;
using Api.Services.Auth;
using Infrastructure.Auth.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api;

/// <summary>
/// Класс настройки сервиса
/// </summary>
public class StartUp
{
    private IWebHostEnvironment Environment { get; }

    private IConfiguration Configuration { get; }

    public StartUp(IWebHostEnvironment env, IConfiguration configuration)
    {
        Environment = env;
        Configuration = configuration;
    }

    /// <summary>
    /// Конфигурация сервисов
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCyberControllers();

        services.AddCyberCors();
        
        if (Environment.IsDevelopment())
        {
            services.AddHealthChecks();
            services.AddSwaggerDocumentation(apiName: "CyberBackup", version: "v1");
        }

        services.AddInfrastructure();

        services.AddScoped<IAuthCookieService, AuthCookieService>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services
            .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
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
                    RoleClaimType = "role"
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Cookies[AuthCookieNames.AccessToken];

                        if (!string.IsNullOrWhiteSpace(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        services.AddCyberMapper(assemblies: Assembly.GetExecutingAssembly());
        services.AddPostgres();
        services.AddCore();
        services.AddApplication();
    }

    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseCyberCors();

        if (Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwaggerDocumentation("CyberBackupApi");
        }
        
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}