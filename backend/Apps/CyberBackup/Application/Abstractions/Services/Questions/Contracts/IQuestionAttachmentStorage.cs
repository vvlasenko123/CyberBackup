namespace Application.Abstractions.Services.Questions.Contracts;

public interface IQuestionAttachmentStorage
{
    Task SaveAsync(string path, Stream content, long length, string contentType, CancellationToken ct);
    Task<Stream> OpenReadAsync(string path, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
}
