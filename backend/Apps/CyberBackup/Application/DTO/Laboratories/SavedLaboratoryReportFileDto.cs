using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РЎРѕС…СЂР°РЅРµРЅРЅС‹Р№ С„Р°Р№Р» РѕС‚С‡РµС‚Р°
/// </summary>
public sealed record SavedLaboratoryReportFileDto(
    string StoragePath,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes);

