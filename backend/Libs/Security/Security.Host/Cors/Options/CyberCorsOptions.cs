using Microsoft.Extensions.Options;

namespace Security.Host.Cors.Options;

/// <summary>
/// Настройки Cors
/// </summary>
public sealed class CyberCorsOptions : IValidateOptions<CyberCorsOptions>
{
    /// <summary>
    /// Разрешенные origin
    /// </summary>
    public string[]? Origins { get; init; }

    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, CyberCorsOptions options)
    {
        if (options.Origins is null || options.Origins.Length == 0)
        {
            return ValidateOptionsResult.Fail("Список origins для CORS не должен быть пустым");
        }

        return ValidateOptionsResult.Success;
    }
}