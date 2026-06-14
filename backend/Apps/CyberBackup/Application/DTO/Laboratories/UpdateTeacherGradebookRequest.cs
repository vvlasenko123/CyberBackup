namespace Application.DTO.Laboratories;

public sealed record UpdateTeacherGradebookRequest
{
    public int LessonsAttended { get; init; }
    public int TotalLessons { get; init; }
    public bool IsExamAllowed { get; init; }
    public bool HasAutomaticGrade { get; init; }
}

