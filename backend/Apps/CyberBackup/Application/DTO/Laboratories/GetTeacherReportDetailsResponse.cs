using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р”РµС‚Р°Р»Рё РѕС‚С‡РµС‚Р° РґР»СЏ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record GetTeacherReportDetailsResponse
{
    public Guid ReportId { get; init; }
    public TeacherReportLaboratoryDto Laboratory { get; init; } = new();
    public GradebookStudentDto Student { get; init; } = new();
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public bool AllowResubmit { get; init; }
    public IReadOnlyCollection<LaboratoryReportVersionDto> Versions { get; init; } = [];
}

