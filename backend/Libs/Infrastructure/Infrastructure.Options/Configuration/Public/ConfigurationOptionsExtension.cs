using Infrastructure.Options.Configuration.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Options.Configuration.Public;

/// <summary>
/// Extension для конфигурирования в других проектах
/// </summary>
public static class ConfigurationOptionsExtension
{
    /// <summary>
    /// Сбиндить валидацию конфигурации
    /// </summary>
    public static void BindConfigurationOptions<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
    {
        optionsBuilder.BindConfiguration(configSectionPath: typeof(TOptions).Name)
            .UseValidationOptions()
            .ValidateOnStart();
    }
}