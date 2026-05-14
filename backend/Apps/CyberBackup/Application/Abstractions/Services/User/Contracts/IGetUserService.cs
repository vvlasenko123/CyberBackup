using Domain.User;
using Infrastructure.Core.Services.Contracts.Get;

namespace Application.Abstractions.Services.User.Contracts;

/// <summary>
/// Сервис получения пользователя
/// </summary>
public interface IGetUserService : IGetService<Guid, UserModel>
{
    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    Task<IReadOnlyCollection<UserModel>> GetAll(CancellationToken cancellationToken);
}