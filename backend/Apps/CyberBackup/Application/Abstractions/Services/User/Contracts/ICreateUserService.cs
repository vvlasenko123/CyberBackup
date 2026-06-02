using Application.DTO.User;

namespace Application.Abstractions.Services.User.Contracts;

/// <summary>
/// Сервис создания пользователя
/// </summary>
public interface ICreateUserService
{
    Task<Guid> Create(UserDto request, Guid currentUserId, CancellationToken token);
}