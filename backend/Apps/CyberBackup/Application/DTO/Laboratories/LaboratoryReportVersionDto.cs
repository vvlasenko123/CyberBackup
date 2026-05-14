using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р’РµСЂСЃРёСЏ РѕС‚С‡РµС‚Р°
/// </summary>
public sealed record LaboratoryReportVersionDto
{
    public Guid VersionId { get; init; }
    public int VersionNumber { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public long FileSizeBytes { get; init; }
    public string? ContentType { get; init; }
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? TeacherComment { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public Guid? CheckedByTeacherId { get; init; }
    public string? CheckedByTeacherFullName { get; init; }
    public DateTimeOffset? CheckedDateUtc { get; init; }
    public string? FileDownloadUrl { get; init; }
}

