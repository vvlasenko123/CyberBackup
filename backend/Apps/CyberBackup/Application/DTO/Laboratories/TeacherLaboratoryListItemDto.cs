using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р­Р»РµРјРµРЅС‚ СЃРїРёСЃРєР° Р»Р°Р±РѕСЂР°С‚РѕСЂРЅС‹С… РґР»СЏ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record TeacherLaboratoryListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public LaboratoryDifficulty Difficulty { get; init; }
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public bool HasFlag { get; init; }
    public bool IsPublished { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public DateTimeOffset? UpdateDateUtc { get; init; }
}

