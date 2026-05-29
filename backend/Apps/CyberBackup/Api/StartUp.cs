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
using Api.Services.Laboratories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;

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
        services.AddScoped<ILaboratoryReportUploadRequestFactory, LaboratoryReportUploadRequestFactory>();
    }

    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    public void Configure(IApplicationBuilder app)
    {
        // Глобальный JSON-обработчик ошибок — должен быть ПЕРВЫМ в пайплайне
        app.UseExceptionHandler(errorApp => errorApp.Run(async ctx =>
        {
            var feature = ctx.Features.Get<IExceptionHandlerFeature>();
            var ex = feature?.Error;

            ctx.Response.ContentType = "application/json; charset=utf-8";

            ctx.Response.StatusCode = ex switch
            {
                InvalidOperationException => 400,
                UnauthorizedAccessException => 403,
                KeyNotFoundException       => 404,
                _                          => 500
            };

            var message = ctx.Response.StatusCode == 500
                ? "Внутренняя ошибка сервера"
                : ex?.Message ?? "Ошибка запроса";

            await ctx.Response.WriteAsJsonAsync(new { message });
        }));

        app.UseRouting();
        app.UseCyberCors();

        if (Environment.IsDevelopment())
        {
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
