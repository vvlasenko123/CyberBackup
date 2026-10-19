using Infrastructure.Database.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Api.Health;

/// <summary>Checks the configured database with an authenticated, bounded query.</summary>
public sealed class PostgresReadinessCheck(IOptions<PostgresOptions> options) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception)
        {
            // Do not expose credentials or connection details in public probe responses/logs.
            return HealthCheckResult.Unhealthy("PostgreSQL is unavailable.");
        }
    }
}
