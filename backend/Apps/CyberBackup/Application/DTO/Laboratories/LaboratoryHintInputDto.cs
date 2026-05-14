using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РџРѕРґСЃРєР°Р·РєР° РІ Р·Р°РїСЂРѕСЃРµ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record LaboratoryHintInputDto
{
    public Guid? Id { get; init; }
    public int OrderNumber { get; init; }
    public string? Title { get; init; }
    public string Text { get; init; } = string.Empty;
    public int PenaltyPoints { get; init; }
}

