using System.Data;

namespace Infrastructure.Database.Connection.Contracts;

/// <summary>
/// Контракт синхронного соединения
/// </summary>
public interface ISyncDbConnection : IDisposable
{
    /// <summary>
    /// Открыть соединение
    /// </summary>
    void Open();

    /// <summary>
    /// Закрыть соединение
    /// </summary>
    void Close();

    /// <summary>
    /// Создать команду
    /// </summary>
    IDbCommand CreateCommand();

    /// <summary>
    /// Начать транзакцию
    /// </summary>
    IDbTransaction BeginTransaction();

    /// <summary>
    /// Начать транзакцию с уровнем изоляции
    /// </summary>
    IDbTransaction BeginTransaction(IsolationLevel isolationLevel);

    /// <summary>
    /// Сменить базу данных
    /// </summary>
    void ChangeDatabase(string databaseName);
}