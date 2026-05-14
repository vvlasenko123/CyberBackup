using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РћС‚РІРµС‚ РѕС‚РєСЂС‹С‚РёСЏ РїРѕРґСЃРєР°Р·РєРё
/// </summary>
public sealed record OpenLaboratoryHintResponse
{
    public Guid HintId { get; init; }
    public int OrderNumber { get; init; }
    public string Text { get; init; } = string.Empty;
    public int PenaltyPoints { get; init; }
    public int TotalPenaltyPoints { get; init; }
    public int AvailablePoints { get; init; }
}

