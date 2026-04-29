using Infrastructure.Database.Base.Constants;
using Infrastructure.Database.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Database.Additional;

/// <summary>
/// Создатель базы в постгрес
/// </summary>
public sealed class PostgresDatabaseCreator
{
    private readonly PostgresOptions _options;

    public PostgresDatabaseCreator(IOptions<PostgresOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Создает бд, если она не была создана
    /// </summary>
    public async Task CreatedAsync(CancellationToken cancellationToken)
    {
        var builder = new NpgsqlConnectionStringBuilder(_options.ConnectionString);

        var databaseName = builder.Database;

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Имя базы данных не должно быть пустым");
        }

        builder.Database = DatabaseConst.Postgres;

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";

        var exists = await command.ExecuteScalarAsync(cancellationToken);

        if (exists is not null)
        {
            return;
        }

        command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}