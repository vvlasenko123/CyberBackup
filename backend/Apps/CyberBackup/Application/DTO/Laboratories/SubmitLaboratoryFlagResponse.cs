using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РћС‚РІРµС‚ СЃРґР°С‡Рё С„Р»Р°РіР°
/// </summary>
public sealed record SubmitLaboratoryFlagResponse
{
    public bool IsCorrect { get; init; }
    public string Message { get; init; } = string.Empty;
    public int EarnedPoints { get; init; }
    public string Status { get; init; } = string.Empty;
}

