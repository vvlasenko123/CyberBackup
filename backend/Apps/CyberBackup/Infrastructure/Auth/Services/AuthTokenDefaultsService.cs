using Application.Abstractions.Services.Auth.Contracts;
using Infrastructure.Auth.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure.Auth.Services;

/// <summary>
/// Сервис значений токена авторизации по умолчанию.
/// </summary>
public sealed class AuthTokenDefaultsService : IAuthTokenDefaultsService
{
    public AuthTokenDefaultsService(IOptions<AuthTokenOptions> options)
    {
        var value = options.Value;

        ClientId = value.ClientId.Trim();

        Scopes = Array.AsReadOnly(
            value.Scopes
                .Select(scope => scope.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }

    /// <inheritdoc />
    public string ClientId { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> Scopes { get; }
}