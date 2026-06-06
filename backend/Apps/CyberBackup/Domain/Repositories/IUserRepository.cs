using Domain.User;
using Domain.User.ValueObjects;

namespace Domain.Repositories;

/// <summary>
/// Пользовательский репозиторий
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Создать пользователя
    /// </summary>
    Task CreateUserAsync(UserModel userModel, CancellationToken cancellationToken);

    /// <summary>
    /// Изменить пользователя
    /// </summary>
    Task UpdateUserAsync(UserModel userModel, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить пользователя по id
    /// </summary>
    Task<UserModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    Task<IReadOnlyCollection<UserModel>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Проверить, занят ли email при регистрации пользователя
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);

    /// <summary>
    /// Проверить, занят ли email другим пользователем
    /// </summary>
    Task<bool> ExistsByEmailForAnotherUserAsync(Guid userId, string email, CancellationToken cancellationToken);

    /// <summary>
    /// Получить пользователя по email
    /// </summary>
    Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    
    /// <summary>
    /// Сменить пароль пользователя
    /// </summary>
    Task ChangePasswordAsync(Guid userId, PasswordHash passwordHash, CancellationToken cancellationToken);

    /// <summary>
    /// Получить ID студентов преподавателя (через группы)
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetStudentIdsByTeacherAsync(Guid teacherId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить ID всех пользователей с заданными ролями
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetUserIdsByRolesAsync(IEnumerable<int> roles, CancellationToken cancellationToken);
}