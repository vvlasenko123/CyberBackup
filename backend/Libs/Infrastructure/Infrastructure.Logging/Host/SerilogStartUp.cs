using System.Runtime.CompilerServices;
using Infrastructure.Logging.Host.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Infrastructure.Logging.Redaction;
[assembly: InternalsVisibleTo("Infrastructure.Hosting")]

namespace Infrastructure.Logging.Host;

/// <summary>
/// Добавелние настроек Serilog
/// </summary>
internal static class SerilogStartUp
{
    /// <summary>
    /// Поключение Serilog
    /// </summary>
    public static IHostBuilder UseCyberSerilog(this IHostBuilder builder)
    {
        // bootstrap для раннего лога ошибок, потом используем финальный логгер
        Log.Logger = new LoggerConfiguration()
            .Enrich.With<TokenRedactingEnricher>()
            .WriteTo.Console(new TokenRedactingTextFormatter())
            .CreateBootstrapLogger();

        builder.UseSerilog(configureLogger: (context, provider, config) =>
        {
            // финальный логгер
            // todo добавить трейсы запросов, чтобы могли различать
            // var traceId = Activity.Current?.TraceId.ToString();
            // var spanId = Activity.Current?.SpanId.ToString();
            config.Configure(context, provider);
        });
        
        return builder;
    }
}
