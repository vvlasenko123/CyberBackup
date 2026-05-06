using Domain.User.Enums;
using Domain.User.ValueObjects;
using Infrastructure.Core.DDD.Aggregate;

namespace Domain.User;

/// <summary>
/// Пользователь
/// </summary>
public sealed class UserModel : AggregateRoot<Guid>
{
    /// <summary>
    /// Почта
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Имя пользователя
    /// </summary>
    public FullName FullName { get; private set; }

    /// <summary>
    /// Пароль
    /// </summary>
    public PasswordHash Password { get; private set; }

    /// <summary>
    /// Роль пользователя
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Активна ли учетная запись
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Нужно ли сменить пароль
    /// </summary>
    public bool MustChangePassword { get; private set; }

    /// <summary>
    /// Кто создал пользователя
    /// </summary>
    public Guid? CreatedBy { get; private set; }

    /// <summary>
    /// Дата создания
    /// </summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>
    /// Дата обновления
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    public UserModel(
        Guid id,
        Email email,
        FullName fullName, 
        PasswordHash password,
        UserRole role,
        bool isActive,
        bool mustChangePassword,
        Guid? createdBy,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) : base(id)
    {
        Email = email;
        FullName = fullName;
        Password = password;
        Role = role;
        IsActive = isActive;
        MustChangePassword = mustChangePassword;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}