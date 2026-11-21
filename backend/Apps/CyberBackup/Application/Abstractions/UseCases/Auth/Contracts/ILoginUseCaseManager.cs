using Application.DTO.Auth;

namespace Application.Abstractions.UseCases.Auth.Contracts;

/// <summary>
/// Менеджер сценария входа.
/// </summary>
public interface ILoginUseCaseManager
{
    /// <summary>
    /// Выполнить вход. Неверные данные дают <c>InvalidPasswordAttemptException</c>,
    /// заблокированный из-за перебора вход — <c>PasswordAttemptsLimitedException</c>.
    /// </summary>
    Task<LoginResultDto> Execute(
        LoginRequestDto request,
        CancellationToken cancellationToken);
}