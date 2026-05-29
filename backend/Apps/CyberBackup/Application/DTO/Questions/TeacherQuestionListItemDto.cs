using Domain.Questions.Enums;

namespace Application.DTO.Questions;

/// <summary>
/// Элемент списка вопросов для преподавателя
/// </summary>
public sealed record TeacherQuestionListItemDto
{
    public Guid Id { get; init; }
    public string StudentFullName { get; init; } = string.Empty;
    public string? GroupName { get; init; }
    public string? LaboratoryTitle { get; init; }
    public string Description { get; init; } = string.Empty;
    public QuestionStatus Status { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
