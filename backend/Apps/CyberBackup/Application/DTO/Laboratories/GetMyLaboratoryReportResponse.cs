using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РћС‚С‡РµС‚ С‚РµРєСѓС‰РµРіРѕ СЃС‚СѓРґРµРЅС‚Р°
/// </summary>
public sealed record GetMyLaboratoryReportResponse
{
    public Guid ReportId { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public bool AllowResubmit { get; init; }
    public IReadOnlyCollection<LaboratoryReportVersionDto> Versions { get; init; } = [];
}

