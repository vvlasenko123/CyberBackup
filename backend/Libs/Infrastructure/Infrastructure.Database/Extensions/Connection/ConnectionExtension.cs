using System.Data;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Database.Extensions.Connection;

/// <summary>
/// Методы расширения
/// </summary>
public static class ConnectionExtension
{
    /// <summary>
    /// Проверка открыто ли соединение
    /// </summary>
    public static async Task EnsureOpenAsync(this IAsyncDbConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection.State is ConnectionState.Open)
        {
            return;
        }

        await connection.OpenAsync(cancellationToken);
    }
}