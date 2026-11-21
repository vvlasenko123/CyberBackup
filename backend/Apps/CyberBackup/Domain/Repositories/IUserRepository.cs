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
    Task<string?> DeleteUserAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Получить состояние аватара пользователя, null - пользователя нет
    /// </summary>
    Task<UserAvatarState?> GetAvatarStateAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Задать аватар и вернуть вытесненный ключ. Строка пользователя блокируется на время транзакции.
    /// Если исход коммита неизвестен, выбрасывается <see cref="AvatarWriteOutcomeUnknownException"/>
    /// </summary>
    Task<AvatarChangeResult> SetAvatarAsync(
        Guid userId,
        string avatarPath,
        DateTimeOffset avatarUpdatedAt,
        CancellationToken cancellationToken);

    /// <summary>
    /// Очистить аватар и вернуть прежний ключ. Строка пользователя блокируется на время транзакции
    /// </summary>
    Task<AvatarChangeResult> ClearAvatarAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Заменить контакты пользователя, false - пользователя нет
    /// </summary>
    Task<bool> UpdateContactsAsync(
        Guid userId,
        PhoneNumber? phone,
        string? telegram,
        string? vk,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken);

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