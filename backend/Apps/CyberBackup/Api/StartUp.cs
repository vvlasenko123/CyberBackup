using System.Reflection;
using Application;
using Infrastructure;
using Infrastructure.AutoMapper;
using Infrastructure.Core;
using Infrastructure.Core.Controllers;
using Infrastructure.Database;
using Infrastructure.Swagger;
using Security.Host.Cors;

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

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}