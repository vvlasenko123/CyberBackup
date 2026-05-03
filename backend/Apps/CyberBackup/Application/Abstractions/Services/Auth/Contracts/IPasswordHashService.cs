namespace Application.Abstractions.Services.Auth.Contracts;

/// <summary>
/// Сервис хэширования пароля.
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// Получить хэш пароля.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Проверить пароль по хэшу.
    /// </summary>
    bool Verify(string password, string passwordHash);
}