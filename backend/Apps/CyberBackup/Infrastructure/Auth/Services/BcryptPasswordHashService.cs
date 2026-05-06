using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services.Auth.Contracts;
using Infrastructure.Auth.Options;
using Infrastructure.Exceptions.User;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Services;

/// <summary>
/// Сервис хэширования паролей через HMACSHA384 и BCrypt.
/// </summary>
public sealed class BcryptPasswordHashService : IPasswordHashService
{
    private readonly PasswordHashOptions _options;

    public BcryptPasswordHashService(IOptions<PasswordHashOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public string Hash(string password)
    {
        var preparedPassword = PreparePassword(password);

        return BCrypt.Net.BCrypt.HashPassword(
            preparedPassword,
            workFactor: _options.WorkFactor);
    }

    /// <inheritdoc />
    public bool Verify(string password, string passwordHash)
    {
        var preparedPassword = PreparePassword(password);

        return BCrypt.Net.BCrypt.Verify(preparedPassword, passwordHash);
    }

    private string PreparePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidPasswordException("Пароль не должен быть пустым");
        }

        var pepperBytes = Convert.FromBase64String(_options.Pepper);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using var hmac = new HMACSHA384(pepperBytes);
        var hashBytes = hmac.ComputeHash(passwordBytes);

        return Convert.ToBase64String(hashBytes);
    }
}