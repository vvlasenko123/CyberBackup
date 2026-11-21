using Application.Abstractions.Services.Auth;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.DTO.Auth;
using Domain.Auth;
using Domain.Repositories;
using Domain.User.ValueObjects;
using Infrastructure.Exceptions.User;

namespace Application.Features.Auth.Login;

/// <inheritdoc />
public sealed class LoginUseCaseManager : ILoginUseCaseManager
{
    private const string InvalidCredentialsMessage = "Неверная почта или пароль";

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAuthTokenDefaultsService _authTokenDefaultsService;
    private readonly IPasswordAttemptLimiter _attemptLimiter;

    public LoginUseCaseManager(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHashService passwordHashService,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IAuthTokenDefaultsService authTokenDefaultsService,
        IPasswordAttemptLimiter attemptLimiter)
    {
        _attemptLimiter = attemptLimiter;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHashService = passwordHashService;
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _authTokenDefaultsService = authTokenDefaultsService;
    }

    /// <inheritdoc />
    public async Task<LoginResultDto> Execute(
        LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);

        // Попытка засчитывается до проверки, в том числе для несуществующей почты:
        // иначе по ответу можно было бы отличить её от настоящей
        var attemptKey = $"login:{email.Value.ToLowerInvariant()}";
        var attempt = await _attemptLimiter.BeginAttemptAsync(attemptKey, cancellationToken);

        if (!attempt.IsAllowed)
        {
            throw new PasswordAttemptsLimitedException(attempt);
        }

        var user = await _userRepository.GetByEmailAsync(email.Value, cancellationToken);

        if (user is null)
        {
            throw new InvalidPasswordAttemptException(InvalidCredentialsMessage, attempt);
        }

        if (!user.IsActive)
        {
            throw new AccountDeactivatedException();
        }

        var passwordIsValid = _passwordHashService.Verify(
            request.Password,
            user.Password.Value);

        if (!passwordIsValid)
        {
            throw new InvalidPasswordAttemptException(InvalidCredentialsMessage, attempt);
        }

        await _attemptLimiter.ResetAsync(attemptKey, cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        var sessionId = UUIDNext.Uuid.NewSequential();
        var roles = Array.AsReadOnly([user.Role.ToString()]);

        var tokenUserData = new TokenUserDataDto(
            SubjectId: user.Id,
            ClientId: _authTokenDefaultsService.ClientId,
            SessionId: sessionId,
            Scopes: _authTokenDefaultsService.Scopes,
            Roles: roles);

        var accessToken = _jwtService.GenerateAccessToken(tokenUserData);
        var refreshToken = _refreshTokenService.GenerateRefreshToken(nowUtc);

        var refreshTokenModel = new RefreshTokenModel(
            id: UUIDNext.Uuid.NewSequential(),
            userId: user.Id,
            tokenHash: refreshToken.RefreshTokenHash,
            sessionId: sessionId,
            createdAtUtc: nowUtc,
            expiresAtUtc: refreshToken.ExpiresAtUtc);

        await _refreshTokenRepository.CreateRefreshTokenAsync(
            refreshTokenModel,
            cancellationToken);

        return new LoginResultDto(
            UserId: user.Id,
            AccessToken: accessToken.AccessToken,
            RefreshToken: refreshToken.RefreshToken,
            ExpiresAt: accessToken.ExpiresAtUtc,
            RefreshTokenExpiresAtUtc: refreshToken.ExpiresAtUtc,
            MustChangePassword: user.MustChangePassword);
    }
}