namespace Infrastructure.Repositories.Models;

/// <summary>
/// Модель пользователя из базы данных.
/// </summary>
internal sealed class UserDbModel
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Почта.
    /// </summary>
    public string Email { get; init; }

    /// <summary>
    /// ФИО.
    /// </summary>
    public string FullName { get; init; }

    /// <summary>
    /// Хэш пароля.
    /// </summary>
    public string Password { get; init; }

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public int Role { get; init; }

    /// <summary>
    /// Признак активности пользователя.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Признак необходимости смены пароля.
    /// </summary>
    public bool MustChangePassword { get; init; }

    /// <summary>
    /// Идентификатор создателя пользователя.
    /// </summary>
    public Guid? CreatedBy { get; init; }

    /// <summary>
    /// Дата создания пользователя.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Дата обновления пользователя.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}