using Domain.Questions.Enums;

namespace Application.DTO.Questions;

/// <summary>
/// Детали вопроса с историей переписки
/// </summary>
public sealed record QuestionDetailDto
{
    public Guid Id { get; init; }
    public string StudentFullName { get; init; } = string.Empty;
    public string? StudentGroupName { get; init; }
    public string? LaboratoryTitle { get; init; }
    public string Description { get; init; } = string.Empty;
    public QuestionStatus Status { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
    /// <summary>Сообщения чата в хронологическом порядке</summary>
    public IReadOnlyCollection<QuestionReplyDto> Messages { get; init; } = [];
}
