namespace Application.DTO.Laboratories;

/// <summary>
/// Элемент рейтинга группы
/// </summary>
public sealed record LeaderboardItemDto
{
    public int Rank { get; init; }
    public Guid StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public int EarnedPoints { get; init; }
    public bool IsCurrentUser { get; init; }
}
