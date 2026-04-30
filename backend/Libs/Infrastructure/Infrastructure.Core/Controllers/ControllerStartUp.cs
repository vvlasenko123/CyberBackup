using Infrastructure.Core.Controllers.Convention;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Core.Controllers;

/// <summary>
/// Включение контроллеров
/// </summary>
public static class ControllerStartUp
{
    /// <summary>
    /// Добавить контроллеры
    /// </summary>
    public static void AddCyberControllers(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Conventions.Add(new BaseControllerRouteConvention());
        });
    }
}