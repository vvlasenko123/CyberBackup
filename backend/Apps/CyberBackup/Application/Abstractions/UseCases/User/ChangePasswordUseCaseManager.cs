using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO.Auth;
using Domain.Repositories;
using Domain.User.ValueObjects;

namespace Application.Abstractions.UseCases.User;

/// <inheritdoc />
public sealed class ChangePasswordUseCaseManager : IChangePasswordUseCaseManager
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHashService _passwordHashService;

    public ChangePasswordUseCaseManager(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordHashService passwordHashService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHashService = passwordHashService;
    }

    /// <inheritdoc />
    public async Task Execute(ChangePasswordDto request, CancellationToken token)
    {
        var currentUser = _jwtService.GetCurrentUser();

        var user = await _userRepository.GetByIdAsync(
            currentUser.UserId,
            token);

        if (user is null)
        {
            throw new InvalidOperationException("Пользователь не найден");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Пользователь отключен");
        }

        var currentPasswordIsValid = _passwordHashService.Verify(
            request.CurrentPassword,
            user.Password.Value);

        if (!currentPasswordIsValid)
        {
            throw new InvalidOperationException("Текущий пароль указан неверно");
        }

        var passwordHash = _passwordHashService.Hash(request.NewPassword);

        await _userRepository.ChangePasswordAsync(
            currentUser.UserId,
            new PasswordHash(passwordHash),
            token);
    }
}