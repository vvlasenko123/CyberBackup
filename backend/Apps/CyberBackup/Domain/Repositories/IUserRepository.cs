using Domain.User;
using Infrastructure.Core.Repositories.Contracts.Create;

namespace Domain.Repositories;

/// <summary>
/// Пользовательский репозиторий
/// </summary>
public interface IUserRepository : ICreateRepository<UserModel, Guid>
{
    /// <summary>
    /// Создать пользователя
    /// </summary>
    Task CreateUserAsync(UserModel userModel, CancellationToken cancellationToken);
    
    /// <summary>
    /// Проверить существование пользователя по email.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
}