using Application.Abstractions.Services.Auth.Contracts;
using Domain.Repositories;
using Infrastructure.Auth.Options;
using Infrastructure.Auth.Services;
using Infrastructure.Database.Migrations.Contracts;
using Infrastructure.Migrations;
using Infrastructure.Options.Configuration.Public;
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
        #region options
        services.AddOptions<JwtOptions>().BindConfigurationOptions();
        services.AddOptions<RefreshTokenOptions>().BindConfigurationOptions();
        services.AddOptions<AuthTokenOptions>().BindConfigurationOptions();
        services.AddOptions<PasswordHashOptions>().BindConfigurationOptions();
        #endregion

        #region migrations
        services.AddTransient<IDatabaseMigration, CreateGroups_202604301230>();
        services.AddTransient<IDatabaseMigration, CreateUsers_202604301231>();
        services.AddTransient<IDatabaseMigration, CreateUserGroups_202604301231>();
        services.AddTransient<IDatabaseMigration, CreateRefreshTokens_202605030001>();
        #endregion

        #region repositories
        services.AddScoped<IUserRepository, UserRepository>();
        #endregion

        #region auth
        services.AddScoped<IPasswordHashService, BcryptPasswordHashService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IAuthTokenDefaultsService, AuthTokenDefaultsService>();
        #endregion
    }
}
