using Infrastructure.Core.UseCases.Contracts.Delete;

namespace Application.Abstractions.UseCases.User.Contracts;

/// <summary>
/// Менеджер удаления пользователя
/// </summary>
public interface IDeleteUserUseCaseManager : IDeleteUseCase<Guid>
{

}