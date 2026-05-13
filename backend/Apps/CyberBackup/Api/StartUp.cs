using System.Reflection;
using Application;
using Infrastructure;
using Infrastructure.AutoMapper;
using Infrastructure.Core;
using Infrastructure.Core.Controllers;
using Infrastructure.Database;
using Infrastructure.Swagger;
using Security.Host.Cors;
using Api.Auth;
using Api.Extensions;
using Application.Abstractions.Services.Calendar.Hubs;
using Infrastucture.S3;
using Api.Services.Auth;
using Application.Abstractions.UseCases.User;
using Microsoft.AspNetCore.Authentication.Cookies;

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

        services.AddHealthChecks();

        if (Environment.IsDevelopment())
        {
            services.AddSwaggerDocumentation(apiName: "CyberBackup", version: "v1");
        }

        services.AddPostgres();
        services.AddInfrastructure();

        services.AddSingleton<ICookieManager, ChunkingCookieManager>();
        services.AddScoped<AppendLoginCookiesFilter>();

        services.AddCyberJwtAuthentication();

        services.AddCyberMapper(assemblies: Assembly.GetExecutingAssembly());
        services.AddMinioStorage();
        services.AddCore();
        services.AddApplication();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
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
            endpoints.MapHub<NotificationHub>("/notification-hub");
        });
    }
}
