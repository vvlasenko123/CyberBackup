using Domain.Laboratories.Enums;

namespace Application.DTO.Laboratories;

/// <summary>
/// РЎС‚СѓРґРµРЅС‚ РІ РІРµРґРѕРјРѕСЃС‚Рё
/// </summary>
public sealed record GradebookStudentDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? GroupName { get; init; }
}

