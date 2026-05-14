using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РџСЂРѕРіСЂРµСЃСЃ РїРѕ Р»Р°Р±РѕСЂР°С‚РѕСЂРЅРѕР№ СЂР°Р±РѕС‚Рµ
/// </summary>
public sealed record MyProgressLaboratoryDto
{
    public Guid LaboratoryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public StudentLaboratoryStatus Status { get; init; }
    public int EarnedPoints { get; init; }
    public int MaxPoints { get; init; }
}

