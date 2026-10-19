using System.Text.RegularExpressions;

namespace Infrastructure.Logging.Redaction;

/// <summary>Redacts authentication tokens in query strings and authorization text.</summary>
internal static partial class TokenRedactor
{
    internal const string Replacement = "[REDACTED]";

    internal static bool IsSensitiveName(string name)
    {
        var normalized = name.Replace("_", "").Replace("-", "");
        return normalized.Equals("AccessToken", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("RefreshToken", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Authorization", StringComparison.OrdinalIgnoreCase);
    }

    internal static string Redact(string text)
    {
        var redacted = QueryParameter().Replace(text, match =>
        {
            string key;
            try { key = Uri.UnescapeDataString(match.Groups["key"].Value); }
            catch (UriFormatException) { return match.Value; }

            return IsSensitiveName(key)
                ? match.Groups["prefix"].Value + match.Groups["key"].Value + "=" + Replacement
                : match.Value;
        });
        return BearerToken().Replace(redacted, "Bearer " + Replacement);
    }

    [GeneratedRegex("(?<prefix>^|[?&\\s])(?<key>[^\\s?&=\\\"']+)=(?<value>[^\\s&#\\\"']*)", RegexOptions.CultureInvariant)]
    private static partial Regex QueryParameter();

    [GeneratedRegex(@"\bBearer\s+[A-Za-z0-9._~+/-]+=*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BearerToken();
}
