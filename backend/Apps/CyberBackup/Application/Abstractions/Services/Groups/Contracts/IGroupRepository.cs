using Application.DTO.Groups;

namespace Application.Abstractions.Services.Groups.Contracts;

/// <summary>
/// Репозиторий групп
/// </summary>
public interface IGroupRepository
{
    /// <summary>Получить список всех групп с количеством участников</summary>
    Task<IReadOnlyCollection<GetGroupListItemDto>> GetGroupsAsync(CancellationToken cancellationToken);

    /// <summary>Получить детали группы вместе со студентами и преподавателями</summary>
    Task<GetGroupDetailsResponse?> GetGroupDetailsAsync(Guid groupId, CancellationToken cancellationToken);

    /// <summary>Создать группу, вернуть её Id</summary>
    Task<Guid> CreateGroupAsync(string name, CancellationToken cancellationToken);

    /// <summary>Переименовать группу</summary>
    Task RenameGroupAsync(Guid groupId, string name, CancellationToken cancellationToken);

    /// <summary>Удалить группу (каскадно чистит teacher_groups и user_groups)</summary>
    Task DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken);

    /// <summary>Получить студентов, не состоящих ни в одной группе</summary>
    Task<IReadOnlyCollection<GroupMemberDto>> GetUngroupedStudentsAsync(CancellationToken cancellationToken);

    /// <summary>Добавить студента в группу (убирает из предыдущей группы)</summary>
    Task AddStudentToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Добавить нескольких студентов в группу одной транзакцией</summary>
    Task AddStudentsToGroupAsync(Guid groupId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

    /// <summary>Убрать студента из группы</summary>
    Task RemoveStudentFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Назначить преподавателя на группу</summary>
    Task AddTeacherToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Назначить нескольких преподавателей на группу одной транзакцией</summary>
    Task AddTeachersToGroupAsync(Guid groupId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

    /// <summary>Снять преподавателя с группы</summary>
    Task RemoveTeacherFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);
}
