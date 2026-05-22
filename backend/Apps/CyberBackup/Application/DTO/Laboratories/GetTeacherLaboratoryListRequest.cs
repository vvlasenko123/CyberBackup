using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р—Р°РїСЂРѕСЃ СЃРїРёСЃРєР° Р»Р°Р±РѕСЂР°С‚РѕСЂРЅС‹С… РґР»СЏ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record GetTeacherLaboratoryListRequest
{
    public string? Block { get; init; }
    public LaboratoryDifficulty? Difficulty { get; init; }
    public bool? IsPublished { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

