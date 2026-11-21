namespace Application.Abstractions.Services.Auth;

/// <summary>
/// Правила защиты от перебора пароля
/// </summary>
/// <param name="MaxAttempts">Сколько попыток разрешено в серии до первой блокировки</param>
/// <param name="Window">Через сколько без попыток серия забывается</param>
/// <param name="BaseDelay">Первая блокировка, дальше каждая следующая попытка удваивает задержку</param>
/// <param name="MaxDelay">Верхняя граница задержки</param>
public sealed record PasswordAttemptPolicy(
    int MaxAttempts,
    TimeSpan Window,
    TimeSpan BaseDelay,
    TimeSpan MaxDelay)
{
    /// <summary>
    /// 5 попыток, серия забывается через 15 минут, блокировка 30 с, 60 с, 120 с, ... до 15 минут
    /// </summary>
    public static PasswordAttemptPolicy Default { get; } = new(
        MaxAttempts: 5,
        Window: TimeSpan.FromMinutes(15),
        BaseDelay: TimeSpan.FromSeconds(30),
        MaxDelay: TimeSpan.FromMinutes(15));
}
