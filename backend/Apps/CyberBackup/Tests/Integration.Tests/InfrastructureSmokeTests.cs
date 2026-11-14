using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure;
using Infrastructure.Database;
using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.HostedService;
using Infrastucture.S3;
using Infrastucture.S3.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace CyberBackup.IntegrationTests;

public sealed class InfrastructureSmokeTests
{
    [Fact]
    public async Task MigratesLaboratorySchemaAndRoundTripsReportFile()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        var cancellationToken = timeout.Token;

        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.Sources.Clear();
        builder.Configuration.AddEnvironmentVariables("CYBERBACKUP_TEST_");
        builder.Services.AddPostgres();
        builder.Services.AddMinioStorage();
        builder.Services.AddInfrastructure();

        using var host = builder.Build();
        var migrations = host.Services.GetServices<IHostedService>()
            .OfType<PostgresAutoMigrationHostedService>()
            .Single();
        await migrations.StartAsync(cancellationToken);

        await using var scope = host.Services.CreateAsyncScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IAsyncDbConnection>();
        await using var connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT count(*) FROM public.laboratory_works";
        Assert.Equal(0L, Assert.IsType<long>(await command.ExecuteScalarAsync(cancellationToken)));

        var reports = scope.ServiceProvider.GetRequiredService<ILaboratoryReportFileStorage>();
        var storage = scope.ServiceProvider.GetRequiredService<MinioObjectStorage>();
        var expected = "CyberBackup integration test report"u8.ToArray();
        using var upload = new MemoryStream(expected);
        var file = await reports.SaveAsync(
            new UploadLaboratoryReportFileDto(upload, "report.txt", "text/plain", upload.Length),
            cancellationToken);

        try
        {
            await using var download = await reports.OpenReadAsync(file.StoragePath, cancellationToken);
            using var actual = new MemoryStream();
            await download.CopyToAsync(actual, cancellationToken);
            Assert.Equal(expected, actual.ToArray());
        }
        finally
        {
            using var cleanupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await storage.DeleteAsync(file.StoragePath, cleanupTimeout.Token);
        }
    }
}
