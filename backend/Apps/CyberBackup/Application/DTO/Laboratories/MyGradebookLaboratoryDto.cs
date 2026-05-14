using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р›Р°Р±РѕСЂР°С‚РѕСЂРЅР°СЏ СЂР°Р±РѕС‚Р° РІ РІРµРґРѕРјРѕСЃС‚Рё
/// </summary>
public sealed record MyGradebookLaboratoryDto
{
    public Guid LaboratoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public LaboratoryReportStatus Status { get; init; }
    public int? Points { get; init; }
    public int MaxPoints { get; init; }
    public string? TeacherComment { get; init; }
}

