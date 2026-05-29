namespace Application.DTO.Questions;

/// <summary>
/// Одно сообщение в чате вопроса (от студента или преподавателя)
/// </summary>
public sealed record QuestionReplyDto
{
    public Guid Id { get; init; }
    public string SenderFullName { get; init; } = string.Empty;
    public bool IsFromTeacher { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; init; }
}
