namespace Infrastructure.Database.Connection.Partial;

/// <summary>
/// Реализация Dispose соеднинения
/// </summary>
public sealed partial class PostgresConnection
{
    /// <inheritdoc />
    public void Dispose()
    {
        if (_connection.IsValueCreated)
        {
            Connection.Dispose();
        }
    }
}