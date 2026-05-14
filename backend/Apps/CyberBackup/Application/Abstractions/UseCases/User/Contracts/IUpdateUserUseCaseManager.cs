using Application.DTO.User;
using Infrastructure.Core.UseCases.Contracts.Update;

namespace Application.Abstractions.UseCases.User.Contracts;

/// <summary>
/// Менеджер изменения пользователя
/// </summary>
public interface IUpdateUserUseCaseManager : IUpdateUseCase<UpdateUserDto>
{

}