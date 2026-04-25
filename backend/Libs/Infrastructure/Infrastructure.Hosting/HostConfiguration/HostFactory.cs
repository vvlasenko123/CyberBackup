using Infrastructure.Hosting.Kestrel;
using Infrastructure.Logging.Host;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Hosting.HostConfiguration;

/// <summary>
/// Фабрика хоста
/// </summary>
public static class HostFactory
{
    /// <summary>
    /// Создание хоста с возможностью переопределения настроек Kestrel
    /// </summary>
    public static IHostBuilder CreateHostBuilder<TStartup>(
        string[] args,
        Action<KestrelServerOptions>? configureKestrelOptions = null)
        where TStartup : class
    {
        return Host.CreateDefaultBuilder(args)
            .UseCyberSerilog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel(options =>
                {
                    webBuilder.UseCyberKestrel();

                    if (configureKestrelOptions is not null)
                    {
                        configureKestrelOptions(options);
                    }
                });

                webBuilder.UseStartup<TStartup>();
            });
    }
}