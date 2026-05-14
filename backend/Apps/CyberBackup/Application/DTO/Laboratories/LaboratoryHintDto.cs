using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РџРѕРґСЃРєР°Р·РєР° Р»Р°Р±РѕСЂР°С‚РѕСЂРЅРѕР№ СЂР°Р±РѕС‚С‹
/// </summary>
public sealed record LaboratoryHintDto
{
    public Guid Id { get; init; }
    public int OrderNumber { get; init; }
    public string? Title { get; init; }
    public int PenaltyPoints { get; init; }
    public bool IsOpened { get; init; }
    public string? Text { get; init; }
}

