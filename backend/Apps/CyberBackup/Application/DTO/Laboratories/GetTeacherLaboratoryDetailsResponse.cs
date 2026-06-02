using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р”РµС‚Р°Р»Рё Р»Р°Р±РѕСЂР°С‚РѕСЂРЅРѕР№ РґР»СЏ РїСЂРµРїРѕРґР°РІР°С‚РµР»СЏ
/// </summary>
public sealed record GetTeacherLaboratoryDetailsResponse
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
    public string Block { get; init; } = string.Empty;
    public int MaxPoints { get; init; }
    public bool HasFlag { get; init; }
    public bool HasExpectedFlag { get; init; }
    public bool IsPublished { get; init; }
    public int SortOrder { get; init; }
    public DateTimeOffset CreateDateUtc { get; init; }
    public DateTimeOffset? UpdateDateUtc { get; init; }
    public DateTimeOffset? DeleteDateUtc { get; init; }
    public DateTimeOffset? DeadlineAtUtc { get; init; }
    public IReadOnlyCollection<LaboratoryHintInputDto> Hints { get; init; } = [];
}

