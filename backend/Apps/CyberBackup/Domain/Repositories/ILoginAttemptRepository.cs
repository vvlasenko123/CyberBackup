using Domain.Auth;

namespace Domain.Repositories;

/// <summary>
/// Хранилище счётчиков попыток ввода пароля
/// </summary>
public interface ILoginAttemptRepository
{
    /// <summary>
    /// Атомарно изменить состояние ключа: <paramref name="update"/> вызывается под блокировкой записи,
    /// поэтому параллельные запросы по одному ключу выполняются строго по очереди
    /// </summary>
    Task UpdateAsync(
        string key,
        Func<LoginAttemptState?, LoginAttemptState> update,
        CancellationToken cancellationToken);

    /// <summary>
    /// Забыть попытки по ключу
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken);
}
