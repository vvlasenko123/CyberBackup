namespace Application.DTO.Questions;

/// <summary>
/// Запрос ответа преподавателя на вопрос
/// </summary>
public sealed record ReplyToQuestionRequest
{
    public string Content { get; init; } = string.Empty;
}
