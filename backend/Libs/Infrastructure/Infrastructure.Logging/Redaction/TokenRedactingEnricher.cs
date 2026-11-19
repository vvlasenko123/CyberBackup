using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Logging.Redaction;

/// <summary>Sanitizes structured log properties before they reach configured sinks.</summary>
public sealed class TokenRedactingEnricher : ILogEventEnricher
{
    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties.ToArray())
        {
            logEvent.AddOrUpdateProperty(new LogEventProperty(property.Key, RedactValue(property.Key, property.Value)));
        }
    }

    private static LogEventPropertyValue RedactValue(string name, LogEventPropertyValue value)
    {
        if (TokenRedactor.IsSensitiveName(name)) return new ScalarValue(TokenRedactor.Replacement);
        return value switch
        {
            ScalarValue { Value: string text } => new ScalarValue(TokenRedactor.Redact(text)),
            SequenceValue sequence => new SequenceValue(sequence.Elements.Select(item => RedactValue("", item))),
            StructureValue structure => new StructureValue(structure.Properties.Select(property =>
                new LogEventProperty(property.Name, RedactValue(property.Name, property.Value))), structure.TypeTag),
            DictionaryValue dictionary => new DictionaryValue(dictionary.Elements.Select(pair =>
                new KeyValuePair<ScalarValue, LogEventPropertyValue>(pair.Key,
                    RedactValue(pair.Key.Value?.ToString() ?? "", pair.Value)))),
            _ => value
        };
    }
}
