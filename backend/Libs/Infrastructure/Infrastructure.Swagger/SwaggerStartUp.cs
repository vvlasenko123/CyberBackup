using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Infrastructure.Swagger;

/// <summary>
/// Подключение Swagger
/// </summary>
public static class SwaggerStartUp
{
    /// <summary>
    /// Регистрация Swagger в DI
    /// </summary>
    public static void AddSwaggerDocumentation(this IServiceCollection services, string? apiName, string? version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiName, nameof(apiName));
        ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo
            {
                Title = apiName,
                Version = version
            });

            var basePath = AppContext.BaseDirectory;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic);

            foreach (var assembly in assemblies)
            {
                var xmlFile = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(basePath, xmlFile);

                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }
        });
    }

    /// <summary>
    /// Подключение middleware Swagger
    /// </summary>
    public static void UseSwaggerDocumentation(this IApplicationBuilder app, string? apiName = "None")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiName, nameof(apiName));

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", apiName);
            options.RoutePrefix = "swagger";
        });
    }
}