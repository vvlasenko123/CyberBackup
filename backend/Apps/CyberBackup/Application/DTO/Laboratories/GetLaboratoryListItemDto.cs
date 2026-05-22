using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р­Р»РµРјРµРЅС‚ СЃРїРёСЃРєР° Р»Р°Р±РѕСЂР°С‚РѕСЂРЅС‹С… СЂР°Р±РѕС‚ РґР»СЏ СЃС‚СѓРґРµРЅС‚Р°
/// </summary>
public sealed record GetLaboratoryListItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public LaboratoryDifficulty Difficulty { get; init; }
    public string DifficultyName { get; init; } = string.Empty;
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public int EarnedPoints { get; init; }
    public StudentLaboratoryStatus Status { get; init; }
    public string StatusName { get; init; } = string.Empty;
    public bool IsCompleted { get; init; }
    public int ProgressPercent { get; init; }
    public int SortOrder { get; init; }
}

