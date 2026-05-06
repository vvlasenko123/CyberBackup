using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Options;

/// <summary>
/// Настройки токена авторизации.
/// </summary>
public sealed class AuthTokenOptions : IValidateOptions<AuthTokenOptions>
{
    /// <summary>
    /// Идентификатор клиента по умолчанию.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Scopes по умолчанию.
    /// </summary>
    public string[] Scopes { get; init; } = [];

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, AuthTokenOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            return ValidateOptionsResult.Fail("Идентификатор клиента не должен быть пустым.");
        }

        if (options.Scopes.Length == 0)
        {
            return ValidateOptionsResult.Fail("Список scopes не должен быть пустым.");
        }

        if (options.Scopes.Any(string.IsNullOrWhiteSpace))
        {
            return ValidateOptionsResult.Fail("Scopes не должны содержать пустые значения.");
        }

        return ValidateOptionsResult.Success;
    }
}