using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.DTO.Auth;
using Domain.Repositories;
using Domain.User;
using Domain.User.Enums;
using Domain.User.ValueObjects;

namespace Application.Features.Auth.Register;

/// <inheritdoc />
public sealed class RegisterUseCase : IRegisterUseCase
{
    private const string DefaultClientId = "web-client";
    private const string DefaultScope = "api";

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;

    public RegisterUseCase(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
    }

    /// <inheritdoc />
    public async Task<RegisterResultDto> Execute(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();

        var exists = await _userRepository.ExistsByEmailAsync(email, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("Пользователь с такой почтой уже существует");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var userId = Guid.CreateVersion7();
        var sessionId = Guid.CreateVersion7();
        var passwordHash = _passwordHashService.Hash(request.Password);

        var user = new UserModel(
            id: userId,
            email: new Email(email),
            fullName: new FullName(request.FullName),
            password: new PasswordHash(passwordHash),
            role: UserRole.Student,
            isActive: true,
            mustChangePassword: false,
            createdBy: userId,
            createdAt: nowUtc,
            updatedAt: nowUtc);

        await _userRepository.CreateUserAsync(user, cancellationToken);

        var tokenUserData = new TokenUserDataDto(
            SubjectId: userId.ToString(),
            ClientId: DefaultClientId,
            SessionId: sessionId,
            Scopes: [DefaultScope],
            Roles: [UserRole.Student.ToString()]);

        var accessToken = _jwtTokenService.GenerateAccessToken(tokenUserData);

        return new RegisterResultDto(
            UserId: userId,
            Role: UserRole.Student,
            AccessToken: accessToken.AccessToken,
            JwtId: accessToken.JwtId,
            SessionId: sessionId,
            ClientId: DefaultClientId,
            Scopes: [DefaultScope],
            Roles: [UserRole.Student.ToString()],
            IssuedAtUtc: accessToken.IssuedAtUtc,
            ExpiresAtUtc: accessToken.ExpiresAtUtc);
    }
}