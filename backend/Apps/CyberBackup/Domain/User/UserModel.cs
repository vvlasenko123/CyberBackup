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

    /// <summary>
    /// Ключ объекта аватара в хранилище, null - аватара нет
    /// </summary>
    public string? AvatarPath { get; private set; }

    /// <summary>
    /// Время последней успешной загрузки аватара, null - аватара нет
    /// </summary>
    public DateTimeOffset? AvatarUpdatedAt { get; private set; }

    /// <summary>
    /// Номер телефона
    /// </summary>
    public PhoneNumber? Phone { get; private set; }

    /// <summary>
    /// Имя пользователя в Telegram без символа @
    /// </summary>
    public string? Telegram { get; private set; }

    /// <summary>
    /// Короткое имя или идентификатор страницы VK
    /// </summary>
    public string? Vk { get; private set; }

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
        DateTimeOffset updatedAt,
        string? avatarPath,
        DateTimeOffset? avatarUpdatedAt,
        PhoneNumber? phone,
        string? telegram,
        string? vk) : base(id)
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
        AvatarPath = avatarPath;
        AvatarUpdatedAt = avatarUpdatedAt;
        Phone = phone;
        Telegram = telegram;
        Vk = vk;
    }
}