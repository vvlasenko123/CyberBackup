using Application.DTO;
using Infrastructure.Core.Services.Contracts.Create;

namespace Application.Abstractions.Services.User.Contracts;

/// <summary>
/// Сервис создания пользователя
/// </summary>
public interface ICreateUserService : ICreateService<UserDto>
{
}