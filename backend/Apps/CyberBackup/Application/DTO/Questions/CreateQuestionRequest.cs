namespace Application.DTO.Questions;

/// <summary>
/// Запрос создания вопроса
/// </summary>
public sealed record CreateQuestionRequest
{
    public string? LaboratoryTitle { get; init; }
    public string Description { get; init; } = string.Empty;
}
