using Domain.Questions.Enums;

namespace Application.DTO.Questions;

/// <summary>
/// Элемент списка вопросов студента
/// </summary>
public sealed record QuestionListItemDto
{
    public Guid Id { get; init; }
    public string? LaboratoryTitle { get; init; }
    public string Description { get; init; } = string.Empty;
    public QuestionStatus Status { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
