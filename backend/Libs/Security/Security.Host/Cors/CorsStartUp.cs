using Infrastructure.Options.Configuration.Public;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Security.Host.Cors.Options;

namespace Security.Host.Cors;

/// <summary>
/// Расширения для настройки CORS
/// </summary>
public static class CorsStartUp
{
    /// <summary>
    /// Регистрирует политику CORS с разрешенными адресами
    /// </summary>
    public static void AddCyberCors(this IServiceCollection services)
    {
        services.AddOptions<CyberCorsOptions>().BindConfigurationOptions();
        services.AddCors();
    }

    /// <summary>
    /// Регистрирует политику CORS с дефолтным разрешением
    /// </summary>
    public static void AddCyberDefaultCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });
    }

    /// <summary>
    /// Подключает политику CORS в конвейер приложения
    /// </summary>
    public static void UseCyberCors(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseCors(policy =>
        {
            var settings = app.ApplicationServices
                .GetRequiredService<IOptions<CyberCorsOptions>>()
                .Value;

            policy
                .WithOrigins(settings.Origins!)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    }
}