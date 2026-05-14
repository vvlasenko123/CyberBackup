using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РџСЂРѕРіСЂРµСЃСЃ С‚РµРєСѓС‰РµРіРѕ СЃС‚СѓРґРµРЅС‚Р°
/// </summary>
public sealed record GetMyProgressResponse
{
    public int TotalLaboratories { get; init; }
    public int CompletedLaboratories { get; init; }
    public int PendingReviewLaboratories { get; init; }
    public int RejectedLaboratories { get; init; }
    public int TotalPoints { get; init; }
    public int EarnedPoints { get; init; }
    public int ProgressPercent { get; init; }
    public IReadOnlyCollection<MyProgressLaboratoryDto> Laboratories { get; init; } = [];
}

