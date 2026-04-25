using System.Data;

namespace Infrastructure.Database.Connection.Partial;

/// <summary>
/// Синхронные методы для постгреса
/// </summary>
public sealed partial class PostgresConnection
{
    /// <inheritdoc />
    public void Open()
    {
        if (Connection.State is ConnectionState.Open)
        {
            return;
        }

        Connection.Open();
    }

    /// <inheritdoc />
    public void Close()
    {
        if (Connection.State is not ConnectionState.Closed)
        {
            Connection.Close();
        }
    }

    /// <inheritdoc />
    public IDbCommand CreateCommand()
    {
        IsOpenConnection();
        return Connection.CreateCommand();
    }

    /// <inheritdoc />
    public IDbTransaction BeginTransaction()
    {
        IsOpenConnection();
        return Connection.BeginTransaction();
    }

    /// <inheritdoc />
    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
        IsOpenConnection();
        return Connection.BeginTransaction(il);
    }

    /// <inheritdoc />
    public void ChangeDatabase(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Имя базы данных не должно быть пустым", nameof(databaseName));
        }

        IsOpenConnection();
        Connection.ChangeDatabase(databaseName);
    }
}