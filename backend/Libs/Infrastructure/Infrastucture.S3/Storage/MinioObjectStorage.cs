using Infrastucture.S3.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastucture.S3.Storage;

/// <summary>
/// Хранилище объектов в Minio
/// </summary>
public sealed class MinioObjectStorage
{
    private readonly IMinioClient _client;
    private readonly MinioOptions _options;

    public MinioObjectStorage(
        IMinioClient client,
        IOptions<MinioOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    /// <summary>
    /// Сохранить объект в Minio
    /// </summary>
    public async Task SaveAsync(
        string objectName,
        Stream content,
        long objectSize,
        string contentType,
        CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithStreamData(content)
            .WithObjectSize(objectSize)
            .WithContentType(contentType);

        await _client.PutObjectAsync(putObjectArgs, cancellationToken);
    }

    /// <summary>
    /// Передать диапазон объекта из Minio непосредственно в целевой поток
    /// </summary>
    public async Task CopyToAsync(
        string objectName,
        Stream destination,
        long offset,
        long length,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(destination);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        cancellationToken.ThrowIfCancellationRequested();
        if (length == 0)
            return;

        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithOffsetAndLength(offset, length)
            .WithCallbackStream((stream, ct) => stream.CopyToAsync(destination, 64 * 1024, ct));

        await _client.GetObjectAsync(getObjectArgs, cancellationToken);
    }

    public Task DeleteAsync(string objectName, CancellationToken cancellationToken)
        => _client.RemoveObjectAsync(new RemoveObjectArgs()
            .WithBucket(_options.BucketName).WithObject(objectName), cancellationToken);

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs()
            .WithBucket(_options.BucketName);

        var exists = await _client.BucketExistsAsync(bucketExistsArgs, cancellationToken);

        if (exists)
        {
            return;
        }

        var makeBucketArgs = new MakeBucketArgs()
            .WithBucket(_options.BucketName);

        await _client.MakeBucketAsync(makeBucketArgs, cancellationToken);
    }
}
