using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РћС‚РІРµС‚ РїСЂРѕРІРµСЂРєРё РѕС‚С‡РµС‚Р°
/// </summary>
public sealed record ReviewLaboratoryReportResponse
{
    public Guid ReportId { get; init; }
    public Guid StudentId { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public bool AllowResubmit { get; init; }
    public DateTimeOffset CheckedDateUtc { get; init; }
}

