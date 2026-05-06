using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Options;

/// <summary>
/// Настройки refresh token.
/// </summary>
public sealed class RefreshTokenOptions : IValidateOptions<RefreshTokenOptions>
{
    /// <summary>
    /// Время жизни refresh token в днях.
    /// </summary>
    public int LifetimeDays { get; init; } = 7;

    /// <summary>
    /// Размер refresh token в байтах.
    /// </summary>
    public int TokenBytes { get; init; } = 64;

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, RefreshTokenOptions options)
    {
        if (options.LifetimeDays <= 0)
        {
            return ValidateOptionsResult.Fail("Время жизни refresh token должно быть больше нуля.");
        }

        if (options.TokenBytes < 32)
        {
            return ValidateOptionsResult.Fail("Refresh token должен содержать минимум 32 случайных байта.");
        }

        return ValidateOptionsResult.Success;
    }
}