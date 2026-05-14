using Application.DTO.User;
using Domain.User;
using Infrastructure.Core.Services.Contracts.Update;

namespace Application.Abstractions.Services.User.Contracts;

/// <summary>
/// Сервис изменения пользователя
/// </summary>
public interface IUpdateUserService : IUpdateService<UpdateUserDto>
{
    /// <summary>
    /// Получить пользователя для изменения
    /// </summary>
    Task<UserModel?> GetForUpdate(Guid id, CancellationToken cancellationToken);
}