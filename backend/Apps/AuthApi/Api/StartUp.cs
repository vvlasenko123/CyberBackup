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

        // todo надо подумать над тем чтобы это прокидывать либо в базовой библиотеке либо в одном сервисе через который будут все ходить
        services.AddCyberCors();
        
        if (Environment.IsDevelopment())
        {
            services.AddHealthChecks();
            services.AddSwaggerDocumentation(apiName: "AuthApi", version: "v1");
        }
        
        // просто наследуемся от Profile и у нас все автоматом работает
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