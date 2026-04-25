using System.Data;
using Infrastructure.Database.Connection;
using Infrastructure.Database.HostedService;
using Infrastructure.Database.Options;
using Infrastructure.Options.Configuration.Public;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Database;

/// <summary>
/// Добавелние настроек базы данных
/// </summary>
public static class DatabaseStartUp
{
    /// <summary>
    /// Добавление Postgres
    /// </summary>
    public static void AddPostgres(this IServiceCollection services)
    {
        services.AddOptions<PostgresOptions>().BindConfigurationOptions();
        services.AddTransient<IDbConnection, PostgresConnection>();
        services.AddHostedService<PostgresAutoMigrationHostedService>();
    }
}