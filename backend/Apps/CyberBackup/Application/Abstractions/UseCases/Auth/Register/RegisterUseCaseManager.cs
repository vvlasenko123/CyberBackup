using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.DTO.Auth;
using Domain.Repositories;
using Domain.User;
using Domain.User.Enums;
using Domain.User.ValueObjects;
using Infrastructure.Exceptions.User;

namespace Application.Abstractions.UseCases.Auth.Register;

/// <inheritdoc />
public sealed class RegisterUseCaseManager : IRegisterUseCaseManager
{
    private const string DefaultRole = nameof(UserRole.Student);

    private static readonly IReadOnlyCollection<string> DefaultRoles = Array.AsReadOnly([DefaultRole]);

    private readonly IAuthTokenDefaultsService _authTokenDefaultsService;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtService _jwtService;

    public RegisterUseCaseManager(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IJwtService jwtService,
        IAuthTokenDefaultsService authTokenDefaultsService)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _jwtService = jwtService;
        _authTokenDefaultsService = authTokenDefaultsService;
    }

    /// <inheritdoc />
    public async Task<RegisterResultDto> Execute(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);

        var exists = await _userRepository.ExistsByEmailAsync(email.Value, cancellationToken);

        if (exists)
        {
            throw new InvalidEmailException("Неверный логин или пароль");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var userId = UUIDNext.Uuid.NewSequential();
        var sessionId = UUIDNext.Uuid.NewSequential();
        var passwordHash = _passwordHashService.Hash(request.Password);

        var user = new UserModel(
            id: userId,
            email: email,
            fullName: new FullName(request.FullName),
            password: new PasswordHash(passwordHash),
            role: UserRole.Student,
            isActive: true,
            mustChangePassword: false,
            createdBy: null,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        await _userRepository.CreateUserAsync(user, cancellationToken);

        var tokenUserData = new TokenUserDataDto(
            SubjectId: userId,
            ClientId: _authTokenDefaultsService.ClientId,
            SessionId: sessionId,
            Scopes: _authTokenDefaultsService.Scopes,
            Roles: DefaultRoles);

        var accessToken = _jwtService.GenerateAccessToken(tokenUserData);

        return new RegisterResultDto(
            UserId: userId,
            Role: UserRole.Student,
            AccessToken: accessToken.AccessToken,
            ExpiresAtUtc: accessToken.ExpiresAtUtc);
    }
}