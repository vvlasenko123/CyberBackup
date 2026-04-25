using System.Data;

namespace Infrastructure.Database.Connection.Contracts;

/// <summary>
/// Асинхронное открытие соединения
/// </summary>
public interface IAsyncDbConnection : IDbConnection
{
    /// <summary>
    /// Открыть соединение асинхронно
    /// </summary>
    Task OpenAsync(CancellationToken cancellationToken);
}