using System.Data;
using System.Diagnostics.CodeAnalysis;
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
    
    /// <summary>
    /// Ленивое создание соединения
    /// </summary>
    private readonly Lazy<NpgsqlConnection> _connection;

    /// <summary>
    /// Текущее соединение
    /// </summary>
    private NpgsqlConnection Connection => _connection.Value;

    /// <inheritdoc />
    [AllowNull]
    public string ConnectionString
    {
        get => _connectionString;
        set => throw new NotSupportedException("Изменение строки подключения запрещено");
    }

    /// <inheritdoc />
    public int ConnectionTimeout => Connection.ConnectionTimeout;

    /// <inheritdoc />
    public string Database => Connection.Database;

    /// <inheritdoc />
    public ConnectionState State => Connection.State;

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

        _connection = new Lazy<NpgsqlConnection>(() => new NpgsqlConnection(_connectionString));
    }

    /// <summary>
    /// Проверка на открытость соединения
    /// </summary>
    private void IsOpenConnection()
    {
        if (Connection.State is not ConnectionState.Open)
        {
            throw new InvalidOperationException("Соединение закрыто");
        }
    }
}
