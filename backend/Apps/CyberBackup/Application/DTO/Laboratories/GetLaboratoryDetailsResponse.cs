using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р”РµС‚Р°Р»Рё Р»Р°Р±РѕСЂР°С‚РѕСЂРЅРѕР№ СЂР°Р±РѕС‚С‹ РґР»СЏ СЃС‚СѓРґРµРЅС‚Р°
/// </summary>
public sealed record GetLaboratoryDetailsResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string ShortDescription { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Narrative { get; init; } = string.Empty;
    public string Goal { get; init; } = string.Empty;
    public string? EnvironmentUrl { get; init; }
    public string? Credentials { get; init; }
    public LaboratoryDifficulty Difficulty { get; init; }
    public string DifficultyName { get; init; } = string.Empty;
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public int EarnedPoints { get; init; }
    public bool HasFlag { get; init; }
    public bool FlagAlreadySubmitted { get; init; }
    public LaboratoryReportStatus ReportStatus { get; init; }
    public bool AllowReportUpload { get; init; }
    public bool CanResubmitReport { get; init; }
    public DateTimeOffset? DeadlineAtUtc { get; init; }
    public IReadOnlyCollection<LaboratoryHintDto> Hints { get; init; } = [];
    public GetMyLaboratoryReportResponse? Report { get; init; }
}

