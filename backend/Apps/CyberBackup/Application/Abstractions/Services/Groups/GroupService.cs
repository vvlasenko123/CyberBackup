using Application.Abstractions.Services.Groups.Contracts;
using Application.DTO.Groups;

namespace Application.Abstractions.Services.Groups;

/// <inheritdoc />
public sealed class GroupService : IGroupService
{
    private readonly IGroupRepository _repository;

    public GroupService(IGroupRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<GetGroupListItemDto>> GetGroupsAsync(CancellationToken cancellationToken)
        => _repository.GetGroupsAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<GetGroupDetailsResponse> GetGroupDetailsAsync(Guid groupId, CancellationToken cancellationToken)
    {
        var result = await _repository.GetGroupDetailsAsync(groupId, cancellationToken);

        if (result is null)
            throw new GroupException("group.not_found", "Группа не найдена");

        return result;
    }

    /// <inheritdoc />
    public Task<Guid> CreateGroupAsync(CreateGroupRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new GroupException("group.name_required", "Название группы не может быть пустым");

        return _repository.CreateGroupAsync(request.Name.Trim(), cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken)
        => _repository.DeleteGroupAsync(groupId, cancellationToken);

    /// <inheritdoc />
    public Task AddStudentToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        => _repository.AddStudentToGroupAsync(groupId, userId, cancellationToken);

    /// <inheritdoc />
    public Task RemoveStudentFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        => _repository.RemoveStudentFromGroupAsync(groupId, userId, cancellationToken);

    /// <inheritdoc />
    public Task AddTeacherToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        => _repository.AddTeacherToGroupAsync(groupId, userId, cancellationToken);

    /// <inheritdoc />
    public Task RemoveTeacherFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        => _repository.RemoveTeacherFromGroupAsync(groupId, userId, cancellationToken);
}
