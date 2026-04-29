using System.Reflection;
using Infrastructure.AutoMapper;
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
        services.AddControllers().AddDataAnnotationsLocalization();

        services.AddHealthChecks();

        services.AddCyberCors();
        
        if (Environment.IsDevelopment())
        {
            services.AddHealthChecks();
            services.AddSwaggerDocumentation(apiName: "CyberBackup", version: "v1");
        }
        
        services.AddCyberMapper(assemblies: Assembly.GetExecutingAssembly());
        services.AddPostgres();
    }

    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    public void Configure(IApplicationBuilder app)
    {
        if (Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwaggerDocumentation();
        }

        app.UseCyberCors();
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks(pattern: "/health");
        });
    }
}