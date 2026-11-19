using Infrastructure.Logging.Redaction;
using Serilog;
using Serilog.Core;
using Serilog.Events;

var cases = new (string Name, Action<ILogger> Write, string[] Secrets)[]
{
    ("request start/finish query properties", log => log.Information("GET {RequestPath}{QueryString}",
        "/notification-hub", "?safe=visible&access_token=sentinel-access-123&refresh_token=sentinel-refresh-456"),
        ["sentinel-access-123", "sentinel-refresh-456"]),
    ("case-insensitive and URL-encoded parameter names", log => log.Information("GET {Url}",
        "/notification-hub?%61ccess%5Ftoken=sentinel-encoded-789&REFRESH_TOKEN=sentinel-uppercase-987"),
        ["sentinel-encoded-789", "sentinel-uppercase-987"]),
    ("named token and authorization properties", log => log.ForContext("AccessToken", "sentinel-property-123")
        .ForContext("Authorization", "Bearer sentinel-header-456").Information("Request accepted"),
        ["sentinel-property-123", "sentinel-header-456"]),
    ("nested structured values", log => log.ForContext("Request", new Dictionary<string, object>
        {
            ["RefreshToken"] = "sentinel-nested-123",
            ["Items"] = new[] { "/notification-hub?access_token=sentinel-array-456" }
        }, destructureObjects: true).Information("Nested request"),
        ["sentinel-nested-123", "sentinel-array-456"]),
    ("literal messages and exception text in stdout", log => log.Error(
        new InvalidOperationException("Failed /notification-hub?access_token=sentinel-exception-123"),
        "Authorization: Bearer sentinel-literal-456"),
        ["sentinel-exception-123", "sentinel-literal-456"])
};

foreach (var test in cases)
{
    var sink = new CaptureSink();
    using var logger = new LoggerConfiguration().Enrich.With<TokenRedactingEnricher>().WriteTo.Sink(sink).CreateLogger();
    test.Write(logger);
    if (sink.Output is null || !(sink.Output.Contains("[REDACTED]") || sink.Properties.Contains("[REDACTED]")))
        throw new Exception(test.Name + ": no redaction marker");
    foreach (var secret in test.Secrets)
        if (sink.Output.Contains(secret)) throw new Exception(test.Name + ": token leaked to stdout");
    if (test.Name != "literal messages and exception text in stdout")
        foreach (var secret in test.Secrets)
            if (sink.Properties.Contains(secret)) throw new Exception(test.Name + ": token leaked in structured properties");
    if (test.Name == "request start/finish query properties" && !sink.Output.Contains("safe=visible"))
        throw new Exception("Non-sensitive query parameter was lost");
    Console.WriteLine("PASS " + test.Name);
}

var control = new CaptureSink();
using (var logger = new LoggerConfiguration().Enrich.With<TokenRedactingEnricher>().WriteTo.Sink(control).CreateLogger())
    logger.Information("GET {RequestPath}{QueryString}", "/public/users", "?page=2&search=student");
if (control.Output is null || !control.Output.Contains("?page=2&search=student"))
    throw new Exception("Ordinary request logging changed unexpectedly");
Console.WriteLine("PASS ordinary request logging is preserved");

sealed class CaptureSink : ILogEventSink
{
    public string? Output { get; private set; }
    public string Properties { get; private set; } = "";

    public void Emit(LogEvent logEvent)
    {
        using var writer = new StringWriter();
        new TokenRedactingTextFormatter().Format(logEvent, writer);
        Output = writer.ToString();
        Properties = string.Join(";", logEvent.Properties.Select(property => property.Value.ToString()));
    }
}
