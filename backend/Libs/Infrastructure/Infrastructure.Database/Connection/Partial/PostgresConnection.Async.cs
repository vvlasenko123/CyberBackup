using System.Data;

namespace Infrastructure.Database.Connection.Partial;

/// <summary>
/// Асинхронные методы для постгреса
/// </summary>
public sealed partial class PostgresConnection
{
    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        if (Connection.State is ConnectionState.Open)
        {
            return;
        }

        await Connection.OpenAsync(cancellationToken);
    }
}