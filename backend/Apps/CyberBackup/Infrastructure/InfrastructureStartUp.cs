using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Calendar;
using Application.Abstractions.Services.Calendar.Contracts;
using Domain.Repositories;
using Infrastructure.Auth.Options;
using Infrastructure.Auth.Services;
using Infrastructure.Database.Migrations.Contracts;
using Infrastructure.Migrations;
using Infrastructure.Options.Configuration.Public;
using Infrastructure.Repositories;
using Infrastructure.Seeds;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Security.Auth.Admin.Options;

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
        services.AddOptions<SuperAdminOptions>().BindConfigurationOptions();
        #endregion

        #region migrations
        services.AddTransient<IDatabaseMigration, CreateGroups_202604301230>();
        services.AddTransient<IDatabaseMigration, CreateUsers_202604301231>();
        services.AddTransient<IDatabaseMigration, CreateUserGroups_202604301231>();
        services.AddTransient<IDatabaseMigration, CreateRefreshTokens_202605030001>();
        services.AddTransient<IDatabaseMigration, CreateCalendarEvents_202605150001>();
        services.AddTransient<IDatabaseMigration, CreateNotifications_202605150002>();
        services.AddTransient<IDatabaseMigration, AddEventTypeToCalendarEvents_202605210001>();
        #endregion

        #region repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICalendarEventRepository, CalendarEventRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        #endregion

        #region auth
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHashService, BcryptPasswordHashService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddSingleton<IAuthTokenDefaultsService, AuthTokenDefaultsService>();
        #endregion

        #region seeds
        services.AddHostedService<SuperAdminSeedHostedService>();
        #endregion

        #region notify
        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, JwtSubUserIdProvider>();
        services.AddScoped<INotificationPushService, SignalRNotificationPushService>();
        #endregion
    }
}
