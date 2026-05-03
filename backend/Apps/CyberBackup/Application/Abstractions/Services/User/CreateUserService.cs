using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.User.Contracts;
using Application.DTO;
using Domain.Repositories;
using Domain.User;
using Domain.User.ValueObjects;
using Infrastructure.Exceptions.User;

namespace Application.Abstractions.Services.User;

/// <inheritdoc />
public class CreateUserService : ICreateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;

    public CreateUserService(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
    }

    /// <inheritdoc />
    public async Task Create(UserDto request, CancellationToken token)
    {
        var passwordHash = _passwordHashService.Hash(request.Password);
        
        var user = new UserModel(
            id: UUIDNext.Uuid.NewSequential(),
            new Email(request.Email),
            new FullName(request.FullName),
            new PasswordHash(passwordHash),
            request.Role,
            isActive: true,
            mustChangePassword: false,
            createdBy: request.CurrentUserId,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow);

        await _userRepository.CreateUserAsync(user, token);
    }
}