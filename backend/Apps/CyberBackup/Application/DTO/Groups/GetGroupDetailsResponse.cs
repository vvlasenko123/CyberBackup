namespace Application.DTO.Groups;

/// <summary>
/// Детали группы с участниками
/// </summary>
public sealed record GetGroupDetailsResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyCollection<GroupMemberDto> Students { get; init; } = [];
    public IReadOnlyCollection<GroupMemberDto> Teachers { get; init; } = [];
}
