namespace Application.DTO.Questions;

public sealed record QuestionAttachmentDto
{
    public Guid Id { get; init; }
    public Guid ReplyId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long SizeBytes { get; init; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string StoragePath { get; init; } = string.Empty;
}

public sealed record UploadQuestionAttachmentDto(Stream Content, string FileName, long Length);
public sealed record QuestionAttachmentDownloadDto(Stream Content, string FileName, string ContentType);
