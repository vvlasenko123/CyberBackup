using Application.DTO.Auth;

namespace Application.Abstractions.UseCases.Auth.Contracts;

/// <summary>
/// UseCase регистрации.
/// </summary>
public interface IRegisterUseCaseManager
{
    /// <summary>
    /// Зарегистрировать пользователя.
    /// </summary>
    Task<RegisterResultDto> Execute(
        RegisterRequestDto request,
        CancellationToken cancellationToken);
}