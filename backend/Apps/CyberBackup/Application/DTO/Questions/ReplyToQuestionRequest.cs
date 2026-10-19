namespace Application.DTO.Questions;

/// <summary>
/// Запрос ответа преподавателя на вопрос
/// </summary>
public sealed record ReplyToQuestionRequest
{
    [System.Text.Json.Serialization.JsonIgnore]
    public IReadOnlyCollection<QuestionAttachmentDto> Attachments { get; init; } = [];
    public string Content { get; init; } = string.Empty;
}
