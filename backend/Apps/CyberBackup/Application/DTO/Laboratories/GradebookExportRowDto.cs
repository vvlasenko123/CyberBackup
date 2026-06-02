namespace Application.DTO.Laboratories;

public sealed record GradebookExportRowDto
{
    public string FullName { get; init; } = string.Empty;
    public string GroupName { get; init; } = string.Empty;
    public int TotalPoints { get; init; }
}
