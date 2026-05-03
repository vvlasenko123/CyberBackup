using Application.Abstractions.Services.User;
using Application.Abstractions.Services.User.Contracts;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.Abstractions.UseCases.User;
using Application.Abstractions.UseCases.User.Contracts;
using Application.Features.Auth.Register;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

/// <summary>
/// Extension Application слоя
/// </summary>
public static class ApplicationStartUp
{
    /// <summary>
    /// Подключение Application слоя
    /// </summary>
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreateUserUseCase, CreateUserUseCase>();
        services.AddScoped<ICreateUserService, CreateUserService>();
        
        services.AddScoped<IRegisterUseCase, RegisterUseCase>();

        //todo после добавления токена надо удалить
        services.AddScoped<ICurrentUser, FakeCurrentUser>();
    }
}
