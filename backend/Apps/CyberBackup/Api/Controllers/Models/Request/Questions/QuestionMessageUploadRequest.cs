namespace Api.Controllers.Models.Request.Questions;

public sealed class QuestionMessageUploadRequest
{
    public string? Content { get; set; }
    public List<IFormFile> Files { get; set; } = [];
}
