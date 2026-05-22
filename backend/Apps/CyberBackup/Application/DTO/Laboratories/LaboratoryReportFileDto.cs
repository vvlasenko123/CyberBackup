using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р¤Р°Р№Р» РІРµСЂСЃРёРё РѕС‚С‡РµС‚Р°
/// </summary>
public sealed record LaboratoryReportFileDto
{
    public string StoragePath { get; init; } = string.Empty;
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

