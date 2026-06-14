using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.User.Contracts;
using Application.DTO;
using Application.DTO.User;
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
    public async Task<Guid> Create(UserDto request, Guid currentUserId, CancellationToken token)
    {
        var email = new Email(request.Email);

        var exists = await _userRepository.ExistsByEmailAsync(email.Value, token);

        if (exists)
        {
            throw new InvalidEmailException("Пользователь с такой почтой уже существует");
        }

        var passwordHash = _passwordHashService.Hash(request.Password);
        
        var user = new UserModel(
            id: UUIDNext.Uuid.NewSequential(),
            email: email,
            fullName: new FullName(request.FullName),
            password: new PasswordHash(passwordHash),
            role: request.Role,
            isActive: true,
            mustChangePassword: true,
            createdBy: currentUserId,
            createdAt: DateTimeOffset.UtcNow,
            updatedAt: DateTimeOffset.UtcNow);

        await _userRepository.CreateUserAsync(user, token);
        return user.Id;
    }
}