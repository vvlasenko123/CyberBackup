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
    /// Открыть объект из Minio на чтение
    /// </summary>
    public async Task<Stream> OpenReadAsync(string objectName, CancellationToken cancellationToken)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        var result = new MemoryStream();

        var getObjectArgs = new GetObjectArgs()
            .WithBucket(_options.BucketName)
            .WithObject(objectName)
            .WithCallbackStream(stream => stream.CopyTo(result));

        await _client.GetObjectAsync(getObjectArgs, cancellationToken);

        result.Position = 0;

        return result;
    }

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
