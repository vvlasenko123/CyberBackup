using Application.Abstractions.Services.Avatars.Contracts;
using Infrastucture.S3.Storage;
using Minio.Exceptions;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class UserAvatarStorage : IUserAvatarStorage
{
    private readonly MinioObjectStorage _storage;

    public UserAvatarStorage(MinioObjectStorage storage)
    {
        _storage = storage;
    }

    /// <inheritdoc />
    public Task SaveAsync(string key, Stream content, long length, string contentType, CancellationToken cancellationToken)
    {
        return _storage.SaveAsync(key, content, length, contentType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        try
        {
            return await _storage.OpenReadAsync(key, cancellationToken);
        }
        catch (ObjectNotFoundException)
        {
            return null;
        }
        catch (ErrorResponseException exception) when (exception.Response?.Code == "NoSuchKey")
        {
            return null;
        }
    }

    /// <inheritdoc />
    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        return _storage.DeleteAsync(key, cancellationToken);
    }
}
