using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services.Laboratories.Contracts;

namespace Application.Abstractions.Services.Laboratories;

/// <inheritdoc />
public sealed class LaboratoryFlagHashService : ILaboratoryFlagHashService
{
    /// <inheritdoc />
    public string HashFlag(string flag)
    {
        var normalized = NormalizeFlag(flag);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        var result = Convert.ToHexString(bytes).ToLowerInvariant();

        return result;
    }

    /// <inheritdoc />
    public bool VerifyFlag(string flag, string expectedHash)
    {
        var actualHash = HashFlag(flag);
        var actualBytes = Encoding.UTF8.GetBytes(actualHash);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedHash);
        var result = actualBytes.Length == expectedBytes.Length
                     && CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);

        return result;
    }

    /// <inheritdoc />
    public string MaskFlag(string flag)
    {
        var normalized = NormalizeFlag(flag);

        if (normalized.Length <= 4)
        {
            var shortMask = new string('*', normalized.Length);

            return shortMask;
        }

        var result = $"{normalized[..2]}***{normalized[^2..]}";

        return result;
    }

    private static string NormalizeFlag(string flag)
    {
        var result = flag.Trim();

        return result;
    }
}
