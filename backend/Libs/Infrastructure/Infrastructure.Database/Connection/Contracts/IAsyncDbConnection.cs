using Npgsql;

namespace Infrastructure.Database.Connection.Contracts;

/// <summary>
/// Асинхронный доступ к базе данных
/// </summary>
public interface IAsyncDbConnection
{
    /// <summary>
    /// Создание соединения
    /// </summary>
    Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken token);

    /// <summary>
    /// Выполнение команды
    /// </summary>
    Task<int> ExecuteAsync(string sql, object? param, CancellationToken cancellationToken);

    /// <summary>
    /// Получение списка
    /// </summary>
    Task<IReadOnlyCollection<T>> QueryAsync<T>(string sql, object? param, CancellationToken cancellationToken);

    /// <summary>
    /// Получение одного значения
    /// </summary>
    Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param, CancellationToken cancellationToken);
}