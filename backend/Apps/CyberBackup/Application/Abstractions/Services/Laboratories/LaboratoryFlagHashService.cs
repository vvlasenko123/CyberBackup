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

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <inheritdoc />
    public bool VerifyFlag(string flag, string expectedHash)
    {
        var actualHash = HashFlag(flag);
        var actualBytes = Encoding.UTF8.GetBytes(actualHash);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedHash);

        return actualBytes.Length == expectedBytes.Length
               && CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    /// <inheritdoc />
    public string MaskFlag(string flag)
    {
        var normalized = NormalizeFlag(flag);

        if (normalized.Length <= 4)
        {
            return new string('*', normalized.Length);
        }

        return $"{normalized[..2]}***{normalized[^2..]}";
    }

    private static string NormalizeFlag(string flag)
    {
        return flag.Trim();
    }
}
