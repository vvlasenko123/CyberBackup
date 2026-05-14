using Infrastructure.Options.Configuration.Public;
using Infrastucture.S3.Options;
using Infrastucture.S3.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace Infrastucture.S3;

/// <summary>
/// minio startup
/// </summary>
public static class MinioStartUp
{
    /// <summary>
    /// Миньо extension
    /// </summary>
    public static void AddMinioStorage(this IServiceCollection services)
    {
        services.AddOptions<MinioOptions>().BindConfigurationOptions();

        // TODO надо избавиться от этого
        services.AddSingleton<IMinioClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<MinioOptions>>().Value;

            var client = new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey);

            if (options.UseSsl)
            {
                client = client.WithSSL();
            }

            return client.Build();
        });

        services.AddSingleton<MinioImageStorage>();
    }
}