namespace Application.Abstractions.Services.Auth.Contracts;

/// <summary>
/// Провайдер значений токена авторизации по умолчанию.
/// </summary>
public interface IAuthTokenDefaultsService
{
    /// <summary>
    /// Идентификатор клиента.
    /// </summary>
    string ClientId { get; }

    /// <summary>
    /// Scopes по умолчанию.
    /// </summary>
    IReadOnlyCollection<string> Scopes { get; }
}