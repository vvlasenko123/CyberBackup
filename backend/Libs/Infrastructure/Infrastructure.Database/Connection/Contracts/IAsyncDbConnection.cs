namespace Infrastructure.Database.Connection.Contracts;

/// <summary>
/// Контракт асинхронного соединения
/// </summary>
public interface IAsyncDbConnection : ISyncDbConnection
{
    /// <summary>
    /// Открыть соединение асинхронно
    /// </summary>
    Task OpenAsync(CancellationToken cancellationToken);
}