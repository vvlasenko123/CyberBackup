using Dapper;
using Npgsql;

namespace Infrastructure.Database.Connection.Partial;

/// <summary>
/// Асинхронные методы для постгреса
/// </summary>
public sealed partial class PostgresConnection
{
    /// <summary>
    /// Создание и открытие соединения
    /// </summary>
    public async Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken token)
    {
        var connection = new NpgsqlConnection(_connectionString);

        await connection.OpenAsync(token);

        return connection;
    }

    /// <summary>
    /// Выполнение команды
    /// </summary>
    public async Task<int> ExecuteAsync(string sql, object? param, CancellationToken token)
    {
        await using var connection = await CreateConnectionAsync(token);
        return await connection.ExecuteAsync(sql, param);
    }

    /// <summary>
    /// Получение списка
    /// </summary>
    public async Task<IReadOnlyCollection<T>> QueryAsync<T>(string sql, object? param, CancellationToken token)
    {
        await using var connection = await CreateConnectionAsync(token);
        var result = await connection.QueryAsync<T>(sql, param);

        return result.ToList();
    }

    /// <summary>
    /// Получение одного значения
    /// </summary>
    public async Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param, CancellationToken token)
    {
        await using var connection = await CreateConnectionAsync(token);
        return await connection.QueryFirstOrDefaultAsync<T>(sql, param);
    }
}