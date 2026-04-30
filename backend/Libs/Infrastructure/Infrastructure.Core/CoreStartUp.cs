using Infrastructure.Core.Controllers.Internal.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Core;

/// <summary>
/// Extension Core либы
/// </summary>
public static class CoreStartUp
{
    public static void AddCore(this IServiceCollection services)
    {
        services.AddScoped<InternalFilter>();
    }
}