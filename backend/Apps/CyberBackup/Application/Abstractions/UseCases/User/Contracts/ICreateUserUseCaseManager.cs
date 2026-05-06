using Application.DTO;
using Infrastructure.Core.UseCases.Contracts.Create;

namespace Application.Abstractions.UseCases.User.Contracts;

/// <summary>
/// UseCase создания пользователя
/// </summary>
public interface ICreateUserUseCaseManager : ICreateUseCase<UserDto>
{
}