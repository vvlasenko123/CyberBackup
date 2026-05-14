using Infrastucture.S3.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace Infrastucture.S3.Storage;

/// <summary>
/// миньо чтобы хранить изображение
/// </summary>
public sealed class MinioImageStorage
{
    /// <summary>
    /// миньо клиент
    /// </summary>
    private readonly IMinioClient _client;
    
    /// <summary>
    /// миньо опции
    /// </summary>
    private readonly MinioOptions _options;

    public MinioImageStorage(
        IMinioClient client,
        IOptions<MinioOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    /// <summary>
    /// Создание бакета, если его нет
    /// </summary>
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