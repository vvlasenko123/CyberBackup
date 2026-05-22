using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р—Р°РїСЂРѕСЃ СЃРїРёСЃРєР° РѕС‚С‡РµС‚РѕРІ РґР»СЏ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record GetTeacherReportListRequest
{
    public LaboratoryReportStatus? Status { get; init; }
    public Guid? LaboratoryId { get; init; }
    public string? Search { get; init; }
    public string? GroupName { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

