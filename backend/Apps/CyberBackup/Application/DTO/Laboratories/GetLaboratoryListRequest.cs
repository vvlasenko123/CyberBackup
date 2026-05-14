using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р—Р°РїСЂРѕСЃ СЃРїРёСЃРєР° Р»Р°Р±РѕСЂР°С‚РѕСЂРЅС‹С… СЂР°Р±РѕС‚ РґР»СЏ СЃС‚СѓРґРµРЅС‚Р°
/// </summary>
public sealed record GetLaboratoryListRequest
{
    public string? Block { get; init; }
    public LaboratoryDifficulty? Difficulty { get; init; }
    public StudentLaboratoryStatus? Status { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

