using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р—Р°РїСЂРѕСЃ РїСЂРѕРІРµСЂРєРё РѕС‚С‡РµС‚Р°
/// </summary>
public sealed record ReviewLaboratoryReportRequest
{
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public string? Comment { get; init; }
    public bool AllowResubmit { get; init; }
}

