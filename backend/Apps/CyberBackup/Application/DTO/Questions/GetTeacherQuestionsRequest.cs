using Domain.Questions.Enums;

namespace Application.DTO.Questions;

/// <summary>
/// Параметры запроса списка вопросов для преподавателя
/// </summary>
public sealed record GetTeacherQuestionsRequest
{
    public QuestionStatus? Status { get; init; }
    public string? LaboratoryTitle { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
