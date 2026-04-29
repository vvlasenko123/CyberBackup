using System.Data;
using Infrastructure.Database.Additional;
using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Connection.Partial;
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
        services.AddTransient<IAsyncDbConnection, PostgresConnection>();
        services.AddTransient<ISyncDbConnection, PostgresConnection>();
        services.AddTransient<PostgresDatabaseCreator>();
        services.AddHostedService<PostgresAutoMigrationHostedService>();
    }
}