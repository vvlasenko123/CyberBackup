namespace Application.DTO.Laboratories;

public sealed record TeacherGradebookItemDto
{
    public Guid StudentId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? GroupName { get; init; }
    public decimal AttendancePercent { get; init; }
    public int LessonsAttended { get; init; }
    public int TotalLessons { get; init; }
    public bool IsExamAllowed { get; init; }
    public bool HasAutomaticGrade { get; init; }
    public int TotalPoints { get; init; }
    public int CompletedLaboratories { get; init; }
    public int TotalLaboratories { get; init; }
}

