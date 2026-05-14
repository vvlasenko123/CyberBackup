using Domain.User;
using Infrastructure.Core.Services.Contracts.Delete;

namespace Application.Abstractions.Services.User.Contracts;

/// <summary>
/// Сервис удаления пользователя
/// </summary>
public interface IDeleteUserService : IDeleteService<Guid>
{
    /// <summary>
    /// Получить пользователя для удаления
    /// </summary>
    Task<UserModel?> GetForDelete(Guid id, CancellationToken cancellationToken);
}