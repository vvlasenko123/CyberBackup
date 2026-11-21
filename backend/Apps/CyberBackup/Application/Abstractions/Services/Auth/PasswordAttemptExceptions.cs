using Application.Abstractions.Services.Auth.Contracts;

namespace Application.Abstractions.Services.Auth;

/// <summary>
/// Пароль неверный; несёт остаток попыток и, если эта попытка включила блокировку, время разблокировки
/// </summary>
public sealed class InvalidPasswordAttemptException : Exception
{
    public InvalidPasswordAttemptException(string message, PasswordAttemptResult attempt)
        : base(Compose(message, attempt))
    {
        RemainingAttempts = attempt.RemainingAttempts;
        LockedUntilUtc = attempt.LockedUntilUtc;
        RetryAfterSeconds = attempt.RetryAfterSeconds;
    }

    /// <summary>
    /// Сколько попыток осталось до блокировки
    /// </summary>
    public int RemainingAttempts { get; }

    /// <summary>
    /// Когда закончится блокировка, если она включилась этой попыткой
    /// </summary>
    public DateTimeOffset? LockedUntilUtc { get; }

    /// <summary>
    /// Через сколько секунд можно повторить; 0, если блокировки нет
    /// </summary>
    public int RetryAfterSeconds { get; }

    private static string Compose(string message, PasswordAttemptResult attempt)
    {
        return attempt.LockedUntilUtc is null
            ? $"{message}. Осталось попыток: {attempt.RemainingAttempts}"
            : $"{message}. Слишком много попыток, повторите через {attempt.RetryAfterSeconds} с";
    }
}

/// <summary>
/// Ввод пароля заблокирован из-за серии неверных попыток; пароль при этом не проверялся
/// </summary>
public sealed class PasswordAttemptsLimitedException : Exception
{
    public PasswordAttemptsLimitedException(PasswordAttemptResult attempt)
        : base($"Слишком много попыток, повторите через {attempt.RetryAfterSeconds} с")
    {
        LockedUntilUtc = attempt.LockedUntilUtc
            ?? throw new ArgumentException("Нет времени разблокировки", nameof(attempt));
        RetryAfterSeconds = attempt.RetryAfterSeconds;
    }

    /// <summary>
    /// Когда закончится блокировка
    /// </summary>
    public DateTimeOffset LockedUntilUtc { get; }

    /// <summary>
    /// Через сколько секунд можно повторить
    /// </summary>
    public int RetryAfterSeconds { get; }
}
