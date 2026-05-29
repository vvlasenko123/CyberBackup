namespace Application.DTO.Laboratories;

/// <summary>
/// Рейтинг группы студентов
/// </summary>
public sealed record GetGroupLeaderboardResponse
{
    /// <summary>Место текущего студента в рейтинге (0 — нет группы)</summary>
    public int CurrentUserRank { get; init; }
    public IReadOnlyCollection<LeaderboardItemDto> Items { get; init; } = [];
}
