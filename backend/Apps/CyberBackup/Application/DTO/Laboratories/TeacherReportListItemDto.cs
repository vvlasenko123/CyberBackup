using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р­Р»РµРјРµРЅС‚ СЃРїРёСЃРєР° РѕС‚С‡РµС‚РѕРІ РґР»СЏ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record TeacherReportListItemDto
{
    public Guid ReportId { get; init; }
    public Guid LaboratoryId { get; init; }
    public string LaboratoryTitle { get; init; } = string.Empty;
    public Guid StudentId { get; init; }
    public string StudentFullName { get; init; } = string.Empty;
    public string? GroupName { get; init; }
    public int CurrentVersionNumber { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public int MaxPoints { get; init; }
    public bool AllowResubmit { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public DateTimeOffset? UpdateDateUtc { get; init; }
    public DateTimeOffset LastSubmitDateUtc { get; init; }
}

