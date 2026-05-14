using Domain.User;
using Infrastructure.Core.UseCases.Contracts.Get;

namespace Application.Abstractions.UseCases.User.Contracts;

/// <summary>
/// Менеджер получения пользователя.
/// </summary>
public interface IGetUserUseCaseManager : IGetUseCase<Guid, UserModel>
{
    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    Task<IReadOnlyCollection<UserModel>> Execute(CancellationToken cancellationToken);
}