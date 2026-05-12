using Application.DTO.Auth;

namespace Application.Abstractions.UseCases.Auth.Contracts;

/// <summary>
/// Менеджер сценария входа.
/// </summary>
public interface ILoginUseCaseManager
{
    /// <summary>
    /// Выполнить вход.
    /// </summary>
    Task<LoginResultDto?> Execute(
        LoginRequestDto request,
        CancellationToken cancellationToken);
}