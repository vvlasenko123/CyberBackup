namespace Application.DTO.Groups;

/// <summary>
/// Запрос массового добавления участников в группу
/// </summary>
public sealed record BulkMembersRequest
{
    public IReadOnlyCollection<Guid> UserIds { get; init; } = [];
}
