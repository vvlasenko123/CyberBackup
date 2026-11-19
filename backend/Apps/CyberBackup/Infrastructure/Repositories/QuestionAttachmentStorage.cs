using Application.Abstractions.Services.Questions.Contracts;
using Infrastucture.S3.Storage;

namespace Infrastructure.Repositories;

public sealed class QuestionAttachmentStorage(MinioObjectStorage storage) : IQuestionAttachmentStorage
{
    public Task SaveAsync(string path, Stream content, long length, string contentType, CancellationToken ct)
        => storage.SaveAsync(path, content, length, contentType, ct);
    public Task<Stream> OpenReadAsync(string path, CancellationToken ct) => storage.OpenReadAsync(path, ct);
    public Task DeleteAsync(string path, CancellationToken ct) => storage.DeleteAsync(path, ct);
}
