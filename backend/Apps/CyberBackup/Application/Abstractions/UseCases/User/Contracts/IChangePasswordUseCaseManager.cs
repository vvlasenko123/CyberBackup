using Application.DTO.Auth;
using Infrastructure.Core.UseCases.Contracts.Update;

namespace Application.Abstractions.UseCases.User.Contracts;

/// <summary>
/// Менеджер смены пароля
/// </summary>
public interface IChangePasswordUseCaseManager : IUpdateUseCase<ChangePasswordDto>
{

}