namespace Application.Abstractions.Services.Auth.Contracts;

/// <summary>
/// Результат учёта попытки ввода пароля
/// </summary>
/// <param name="IsAllowed">Попытку можно проверять; <c>false</c> — ввод заблокирован, пароль проверять нельзя</param>
/// <param name="RemainingAttempts">Сколько попыток останется, если эта окажется неверной</param>
/// <param name="LockedUntilUtc">Когда закончится блокировка; для разрешённой попытки заполнено, если неверный ввод её включит</param>
/// <param name="RetryAfterSeconds">Через сколько секунд можно повторить; 0, если блокировки нет</param>
public sealed record PasswordAttemptResult(
    bool IsAllowed,
    int RemainingAttempts,
    DateTimeOffset? LockedUntilUtc,
    int RetryAfterSeconds);

/// <summary>
/// Защита от перебора пароля: лимит попыток за интервал и нарастающая задержка после серии неверных
/// </summary>
public interface IPasswordAttemptLimiter
{
    /// <summary>
    /// Засчитать попытку до проверки пароля. Попытка учитывается заранее, чтобы параллельные запросы
    /// не могли обойти лимит; после успешной проверки счётчик сбрасывается через <see cref="ResetAsync"/>
    /// </summary>
    Task<PasswordAttemptResult> BeginAttemptAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Сбросить счётчик после успешного ввода пароля
    /// </summary>
    Task ResetAsync(string key, CancellationToken cancellationToken);
}
