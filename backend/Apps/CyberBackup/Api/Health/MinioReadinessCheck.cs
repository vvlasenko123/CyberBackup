using Microsoft.Extensions.Diagnostics.HealthChecks;
using Minio;

namespace Api.Health;

/// <summary>Checks S3 availability and the configured credentials without writing objects.</summary>
public sealed class MinioReadinessCheck(IMinioClient client) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // The current Compose uses MinIO root credentials. This probe requires ListAllMyBuckets.
            // It also succeeds before the application's bucket is created by its first upload.
            await client.ListBucketsAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception)
        {
            return HealthCheckResult.Unhealthy("MinIO is unavailable or authentication failed.");
        }
    }
}
