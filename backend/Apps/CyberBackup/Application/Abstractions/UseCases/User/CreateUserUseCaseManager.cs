using Application.Abstractions.Services.User.Contracts;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO;
using Domain.User.Enums;
using Microsoft.AspNetCore.Authentication;

namespace Application.Abstractions.UseCases.User;

/// <inheritdoc />
public class CreateUserUseCaseManager : ICreateUserUseCaseManager
{
    private readonly ICreateUserService _createUserService;
    private readonly ICurrentUser _currentUser;

    public CreateUserUseCaseManager(
        ICreateUserService createUserService,
        ICurrentUser currentUser)
    {
        _createUserService = createUserService;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public async Task Execute(UserDto request, CancellationToken cancellationToken)
    {
        // todo currentUser получаем из токена
        Validate(request);
        await _createUserService.Create(request, cancellationToken);
    }

    /// <summary>
    /// Проверка прав пользователя
    /// </summary>
    private void Validate(UserDto request)
    {
        if (_currentUser.Role != UserRole.Admin && request.Role != UserRole.Student)
        {
            throw new InvalidOperationException("Недостаточно прав для создания пользователя с такой ролью");
        }
    }
}

// todo после добавленя токена надо удалить
public sealed class FakeCurrentUser : ICurrentUser
{
    public Guid UserId { get; } = Guid.NewGuid();

    public UserRole Role { get; } = UserRole.Admin;
}

public interface ICurrentUser
{
    Guid UserId { get; }
    UserRole Role { get; }
}