using Application.Abstractions.Services.Auth.Contracts;
using Domain.Repositories;
using Domain.User;
using Domain.User.Enums;
using Domain.User.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Security.Auth.Admin.Constants;
using Security.Auth.Admin.Options;

namespace Infrastructure.Seeds;

/// <summary>
/// Seed суперадминистратора
/// </summary>
public sealed class SuperAdminSeedHostedService : IHostedService
{
    /// <summary>
    /// Провайдер сервисов
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Настройки суперадминистратора
    /// </summary>
    private readonly SuperAdminOptions _options;

    /// <summary>
    /// Логгер
    /// </summary>
    private readonly ILogger<SuperAdminSeedHostedService> _logger;

    public SuperAdminSeedHostedService(
        IServiceProvider serviceProvider,
        IOptions<SuperAdminOptions> options,
        ILogger<SuperAdminSeedHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Запуск seed
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        var email = new Email(_options.Email);

        var exists = await userRepository.ExistsByEmailAsync(email.Value, cancellationToken);

        if (exists)
        {
            _logger.LogInformation("Администратор уже было создан, завершение сида");
            return;
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var userId = UUIDNext.Uuid.NewSequential();
        var passwordHash = passwordHashService.Hash(_options.Password);

        var user = new UserModel(
            id: userId,
            email: email,
            fullName: new FullName(_options.FullName),
            password: new PasswordHash(passwordHash),
            role: UserRole.SuperAdmin,
            isActive: true,
            mustChangePassword: false,
            createdBy: null,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        await userRepository.CreateUserAsync(user, cancellationToken);

        _logger.LogInformation("Администратор создан");
    }

    /// <summary>
    /// Остановка seed
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}