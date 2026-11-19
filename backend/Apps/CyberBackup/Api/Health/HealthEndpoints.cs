using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Api.Health;

/// <summary>Separates dependency readiness from process liveness.</summary>
public static class HealthEndpoints
{
    /// <summary>Registers bounded dependency checks.</summary>
    public static void AddReadinessChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<PostgresReadinessCheck>("postgres", tags: ["ready"], timeout: TimeSpan.FromSeconds(4))
            .AddCheck<MinioReadinessCheck>("minio", tags: ["ready"], timeout: TimeSpan.FromSeconds(4));
    }

    /// <summary>Maps probes; /health remains an alias for readiness.</summary>
    public static void MapCyberHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
        var readiness = new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            ResponseWriter = (httpContext, report) => httpContext.Response.WriteAsJsonAsync(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.ToDictionary(entry => entry.Key, entry => entry.Value.Status.ToString())
            })
        };
        endpoints.MapHealthChecks("/health/ready", readiness);
        endpoints.MapHealthChecks("/health", readiness);
    }
}
