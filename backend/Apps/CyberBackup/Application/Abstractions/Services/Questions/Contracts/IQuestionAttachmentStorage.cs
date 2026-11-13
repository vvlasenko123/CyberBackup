namespace Application.Abstractions.Services.Questions.Contracts;

public interface IQuestionAttachmentStorage
{
    Task SaveAsync(string path, Stream content, long length, string contentType, CancellationToken ct);
    Task CopyToAsync(string path, Stream destination, long offset, long length, CancellationToken ct);
    Task DeleteAsync(string path, CancellationToken ct);
}
