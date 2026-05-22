using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р—Р°РїСЂРѕСЃ СЃРґР°С‡Рё С„Р»Р°РіР°
/// </summary>
public sealed record SubmitLaboratoryFlagRequest
{
    public string Flag { get; init; } = string.Empty;
}

