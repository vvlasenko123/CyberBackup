namespace Application.DTO.Groups;

/// <summary>
/// Участник группы (студент или преподаватель)
/// </summary>
public sealed record GroupMemberDto
{
    public Guid UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
