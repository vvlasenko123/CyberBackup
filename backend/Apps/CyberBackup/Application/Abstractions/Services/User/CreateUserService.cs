using System.Security.Cryptography;
using System.Text;
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

    public CreateUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task Create(UserDto request, CancellationToken token)
    {
        var passwordHash = HashPassword(request.Password);
        
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
    
    /// <summary>
    /// Хэширование пароля
    /// </summary>
    private static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidPasswordException("Пароль не должен быть пустым");
        }

        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }
}