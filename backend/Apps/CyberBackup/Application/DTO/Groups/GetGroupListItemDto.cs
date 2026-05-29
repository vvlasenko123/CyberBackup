namespace Application.DTO.Groups;

/// <summary>
/// Элемент списка групп
/// </summary>
public sealed record GetGroupListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int StudentCount { get; init; }
    public int TeacherCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
