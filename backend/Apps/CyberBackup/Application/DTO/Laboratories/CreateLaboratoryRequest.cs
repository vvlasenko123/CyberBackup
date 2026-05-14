using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// Р—Р°РїСЂРѕСЃ СЃРѕР·РґР°РЅРёСЏ Р»Р°Р±РѕСЂР°С‚РѕСЂРЅРѕР№
/// </summary>
public record CreateLaboratoryRequest
{
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
    public string? ExpectedFlag { get; init; }
    public bool IsPublished { get; init; }
    public int SortOrder { get; init; }
    public IReadOnlyCollection<LaboratoryHintInputDto> Hints { get; init; } = [];
}

