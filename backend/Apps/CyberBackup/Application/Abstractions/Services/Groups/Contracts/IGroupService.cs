using Application.DTO.Groups;

namespace Application.Abstractions.Services.Groups.Contracts;

/// <summary>
/// Сервис управления группами
/// </summary>
public interface IGroupService
{
    /// <summary>Получить список всех групп</summary>
    Task<IReadOnlyCollection<GetGroupListItemDto>> GetGroupsAsync(CancellationToken cancellationToken);

    /// <summary>Получить детали группы</summary>
    Task<GetGroupDetailsResponse> GetGroupDetailsAsync(Guid groupId, CancellationToken cancellationToken);

    /// <summary>Создать группу</summary>
    Task<Guid> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken);

    /// <summary>Удалить группу</summary>
    Task DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken);

    /// <summary>Получить студентов, не состоящих ни в одной группе</summary>
    Task<IReadOnlyCollection<GroupMemberDto>> GetUngroupedStudentsAsync(CancellationToken cancellationToken);

    /// <summary>Добавить студента в группу</summary>
    Task AddStudentToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Добавить нескольких студентов в группу</summary>
    Task AddStudentsToGroupAsync(Guid groupId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

    /// <summary>Убрать студента из группы</summary>
    Task RemoveStudentFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Назначить преподавателя на группу</summary>
    Task AddTeacherToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);

    /// <summary>Назначить нескольких преподавателей на группу</summary>
    Task AddTeachersToGroupAsync(Guid groupId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken);

    /// <summary>Снять преподавателя с группы</summary>
    Task RemoveTeacherFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken);
}
