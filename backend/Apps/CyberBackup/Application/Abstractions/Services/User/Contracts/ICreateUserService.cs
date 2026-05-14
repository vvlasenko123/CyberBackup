using Application.DTO;
using Application.DTO.User;
using Infrastructure.Core.Services.Contracts.Create;

namespace Application.Abstractions.Services.User.Contracts;

/// <summary>
/// Сервис создания пользователя
/// </summary>
public interface ICreateUserService : ICreateService<UserDto>
{
}