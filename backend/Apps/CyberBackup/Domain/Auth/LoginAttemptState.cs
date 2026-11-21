namespace Domain.Auth;

/// <summary>
/// Состояние попыток ввода пароля для одного ключа (почта при входе, пользователь при смене пароля)
/// </summary>
/// <param name="FailedCount">Число засчитанных попыток в текущей серии</param>
/// <param name="LastAttemptAtUtc">Время последней попытки</param>
/// <param name="LockedUntilUtc">До какого момента ввод заблокирован; <c>null</c>, если блокировки нет</param>
public sealed record LoginAttemptState(
    int FailedCount,
    DateTimeOffset LastAttemptAtUtc,
    DateTimeOffset? LockedUntilUtc);
