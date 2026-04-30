using Domain.Repositories;
using Infrastructure.Database.Migrations.Contracts;
using Infrastructure.Migrations;
using Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Extension инфраструктурного слоя
/// </summary>
public static class InfrastructureStartUp
{
    /// <summary>
    /// Подключение инфраструктурного слоя
    /// </summary>
    public static void AddInfrastructure(this IServiceCollection services)
    {
        # region migrations
        services.AddTransient<IDatabaseMigration, CreateGroups_202604301230>();
        services.AddTransient<IDatabaseMigration, CreateUsers_202604301231>();
        services.AddTransient<IDatabaseMigration, CreateUserGroups_202604301231>();
        # endregion
        
        # region repositories
        services.AddScoped<IUserRepository, UserRepository>();
        # endregion
    }
}
