using Infrastructure.Exceptions.User;

namespace Domain.UserGroups;

/// <summary>
/// Связь пользователя и группы (many-to-many)
/// </summary>
public sealed class UserGroups
{
    /// <summary>
    /// айди пользователя
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// айди группы
    /// </summary>
    public Guid GroupId { get; }

    /// <summary>
    /// Создает связь пользователя и группы
    /// </summary>
    public UserGroups(Guid userId, Guid groupId)
    {
        if (userId == Guid.Empty)
        {
            throw new InvalidIdentifierException("UserId не должен быть пустым");
        }

        if (groupId == Guid.Empty)
        {
            throw new InvalidIdentifierException("GroupId не должен быть пустым");
        }

        UserId = userId;
        GroupId = groupId;
    }
}