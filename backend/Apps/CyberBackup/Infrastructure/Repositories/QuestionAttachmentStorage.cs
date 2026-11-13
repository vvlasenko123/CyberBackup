using Application.Abstractions.Services.Questions.Contracts;
using Infrastucture.S3.Storage;

namespace Infrastructure.Repositories;

public sealed class QuestionAttachmentStorage(MinioObjectStorage storage) : IQuestionAttachmentStorage
{
    public Task SaveAsync(string path, Stream content, long length, string contentType, CancellationToken ct)
        => storage.SaveAsync(path, content, length, contentType, ct);
    public Task CopyToAsync(string path, Stream destination, long offset, long length, CancellationToken ct)
        => storage.CopyToAsync(path, destination, offset, length, ct);
    public Task DeleteAsync(string path, CancellationToken ct) => storage.DeleteAsync(path, ct);
}
