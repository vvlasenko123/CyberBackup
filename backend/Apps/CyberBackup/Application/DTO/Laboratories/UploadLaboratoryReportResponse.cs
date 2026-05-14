using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РћС‚РІРµС‚ Р·Р°РіСЂСѓР·РєРё РѕС‚С‡РµС‚Р°
/// </summary>
public sealed record UploadLaboratoryReportResponse
{
    public Guid ReportId { get; init; }
    public Guid VersionId { get; init; }
    public int VersionNumber { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
}

