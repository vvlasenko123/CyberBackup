using Application.Abstractions.Services.Auth;

namespace Api.Errors;

/// <summary>
/// Тела ответов защиты от перебора пароля: остаток попыток и время до разблокировки
/// </summary>
public static class PasswordAttemptResponses
{
    /// <summary>
    /// Код ошибки: неверная почта или пароль при входе
    /// </summary>
    public const string InvalidCredentials = "auth.invalid_credentials";

    /// <summary>
    /// Код ошибки: неверный текущий пароль при смене пароля
    /// </summary>
    public const string InvalidPassword = "auth.invalid_password";

    /// <summary>
    /// Код ошибки: ввод пароля заблокирован из-за перебора
    /// </summary>
    public const string RateLimited = "auth.rate_limited";

    /// <summary>
    /// Тело ответа на неверный пароль
    /// </summary>
    public static object Invalid(string code, InvalidPasswordAttemptException exception)
    {
        return new
        {
            code,
            message = exception.Message,
            remainingAttempts = exception.RemainingAttempts,
            retryAfterSeconds = exception.LockedUntilUtc is null ? (int?)null : exception.RetryAfterSeconds,
            unlockAtUtc = exception.LockedUntilUtc
        };
    }

    /// <summary>
    /// Тело ответа 429 на заблокированный ввод
    /// </summary>
    public static object Limited(PasswordAttemptsLimitedException exception)
    {
        return new
        {
            code = RateLimited,
            message = exception.Message,
            remainingAttempts = 0,
            retryAfterSeconds = exception.RetryAfterSeconds,
            unlockAtUtc = (DateTimeOffset?)exception.LockedUntilUtc
        };
    }
}
