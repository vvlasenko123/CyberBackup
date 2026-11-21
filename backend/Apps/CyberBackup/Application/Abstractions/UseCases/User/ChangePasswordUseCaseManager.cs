using Application.Abstractions.Services.Auth;
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
    private readonly IPasswordAttemptLimiter _attemptLimiter;

    public ChangePasswordUseCaseManager(
        IUserRepository userRepository,
        IJwtService jwtService,
        IPasswordHashService passwordHashService,
        IPasswordAttemptLimiter attemptLimiter)
    {
        _attemptLimiter = attemptLimiter;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _passwordHashService = passwordHashService;
    }

    /// <inheritdoc />
    public async Task Execute(ChangePasswordDto request, CancellationToken token)
    {
        var currentUser = _jwtService.GetCurrentUser();

        // Текущий пароль тоже можно перебирать, если украден токен, поэтому лимит тот же, что при входе
        var attemptKey = $"password-change:{currentUser.UserId}";
        var attempt = await _attemptLimiter.BeginAttemptAsync(attemptKey, token);

        if (!attempt.IsAllowed)
        {
            throw new PasswordAttemptsLimitedException(attempt);
        }

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
            throw new InvalidPasswordAttemptException("Текущий пароль указан неверно", attempt);
        }

        await _attemptLimiter.ResetAsync(attemptKey, token);

        var passwordHash = _passwordHashService.Hash(request.NewPassword);

        await _userRepository.ChangePasswordAsync(
            currentUser.UserId,
            new PasswordHash(passwordHash),
            token);
    }
}