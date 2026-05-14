using Microsoft.Extensions.Options;
using Security.Auth.Admin.Constants;

namespace Security.Auth.Admin.Options;

/// <summary>
/// Настройки суперадминистратора
/// </summary>
/// <remarks>Для валидации должен быть public</remarks>
public sealed class SuperAdminOptions : IValidateOptions<SuperAdminOptions>
{
    /// <summary>
    /// Почта
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// ФИО
    /// </summary>
    public string FullName { get; init; } = string.Empty;
    
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, SuperAdminOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Email))
        {
            return ValidateOptionsResult.Fail(SecurityText.InvalidEmailOrPassword);
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            return ValidateOptionsResult.Fail(SecurityText.InvalidEmailOrPassword);
        }

        if (string.IsNullOrWhiteSpace(options.FullName))
        {
            return ValidateOptionsResult.Fail(SecurityText.InvalidEmailOrPassword);
        }

        return ValidateOptionsResult.Success;
    }
}