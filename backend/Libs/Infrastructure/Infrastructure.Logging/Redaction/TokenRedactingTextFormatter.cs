using System.Globalization;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Infrastructure.Logging.Redaction;

/// <summary>Also sanitizes rendered messages and exception text written to container stdout.</summary>
public sealed class TokenRedactingTextFormatter : ITextFormatter
{
    private readonly MessageTemplateTextFormatter _inner = new(
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public void Format(LogEvent logEvent, TextWriter output)
    {
        using var buffer = new StringWriter(CultureInfo.InvariantCulture);
        _inner.Format(logEvent, buffer);
        output.Write(TokenRedactor.Redact(buffer.ToString()));
    }
}
