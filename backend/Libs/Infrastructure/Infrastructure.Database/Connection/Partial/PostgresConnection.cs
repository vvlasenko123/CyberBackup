using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Infrastructure.Database.Connection.Partial;

/// <summary>
/// Постгрес на всякий случай если решим что-то менять внутренне
/// </summary>
public sealed partial class PostgresConnection : IAsyncDbConnection
{
    /// <summary>
    /// Строка подключения
    /// </summary>
    private readonly string _connectionString;

    public PostgresConnection(IOptions<PostgresOptions> options)
    {
        var connectionString = options.Value.ConnectionString;

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            _connectionString = builder.ConnectionString;
        }
        catch (Exception ex) when (ex is NpgsqlException or FormatException or ArgumentException)
        {
            throw new ArgumentException("Некорректная строка подключения к Postgres", nameof(options), ex);
        }
    }
}