using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.User.Contracts;
using Application.DTO.User;
using Domain.Repositories;
using Domain.User;
using Domain.User.ValueObjects;
using Infrastructure.Exceptions.User;

namespace Application.Abstractions.Services.User;

/// <inheritdoc />
public sealed class UpdateUserService : IUpdateUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;

    public UpdateUserService(
        IUserRepository userRepository,
        IPasswordHashService passwordHashService)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
    }

    /// <inheritdoc />
    public async Task<UserModel?> GetForUpdate(Guid id, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Update(UpdateUserDto request, CancellationToken cancellationToken)
    {
        var email = new Email(request.Email);

        var emailExists = await _userRepository.ExistsByEmailForAnotherUserAsync(
            request.Id,
            email.Value,
            cancellationToken);

        if (emailExists)
        {
            throw new InvalidEmailException("Пользователь с такой почтой уже существует");
        }

        var user = await _userRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Пользователь не найден");
        }

        var password = user.Password;

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var passwordHash = _passwordHashService.Hash(request.Password);
            password = new PasswordHash(passwordHash);
        }

        var updatedUser = new UserModel(
            id: user.Id,
            email: email,
            fullName: new FullName(request.FullName),
            password: password,
            role: request.Role,
            isActive: request.IsActive,
            mustChangePassword: request.MustChangePassword,
            createdBy: user.CreatedBy,
            createdAt: user.CreatedAt,
            updatedAt: DateTimeOffset.UtcNow);

        await _userRepository.UpdateUserAsync(updatedUser, cancellationToken);
    }
}