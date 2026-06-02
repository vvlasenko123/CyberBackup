using Application.DTO.User;

namespace Application.Abstractions.UseCases.User.Contracts;

/// <summary>
/// UseCase создания пользователя
/// </summary>
public interface ICreateUserUseCaseManager
{
    Task<Guid> Execute(UserDto request, CancellationToken cancellationToken);
}