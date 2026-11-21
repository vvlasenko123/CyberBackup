using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services.Auth.Contracts;
using Domain.Auth;
using Domain.Repositories;

namespace Application.Abstractions.Services.Auth;

/// <inheritdoc />
public sealed class PasswordAttemptLimiter : IPasswordAttemptLimiter
{
    private const int MaxDoublings = 30;

    private readonly ILoginAttemptRepository _repository;
    private readonly TimeProvider _timeProvider;
    private readonly PasswordAttemptPolicy _policy;

    public PasswordAttemptLimiter(
        ILoginAttemptRepository repository,
        TimeProvider timeProvider,
        PasswordAttemptPolicy policy)
    {
        _repository = repository;
        _timeProvider = timeProvider;
        _policy = policy;
    }

    /// <inheritdoc />
    public async Task<PasswordAttemptResult> BeginAttemptAsync(string key, CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        PasswordAttemptResult? result = null;

        await _repository.UpdateAsync(HashKey(key), stored =>
        {
            var state = Actual(stored, now);

            if (state?.LockedUntilUtc > now)
            {
                result = new PasswordAttemptResult(
                    false,
                    0,
                    state.LockedUntilUtc,
                    SecondsUntil(state.LockedUntilUtc.Value, now));

                return state;
            }

            var failedCount = (state?.FailedCount ?? 0) + 1;
            DateTimeOffset? lockedUntil = failedCount >= _policy.MaxAttempts
                ? now + DelayFor(failedCount)
                : null;

            result = new PasswordAttemptResult(
                true,
                Math.Max(0, _policy.MaxAttempts - failedCount),
                lockedUntil,
                lockedUntil is null ? 0 : SecondsUntil(lockedUntil.Value, now));

            return new LoginAttemptState(failedCount, now, lockedUntil);
        }, cancellationToken);

        return result!;
    }

    /// <inheritdoc />
    public Task ResetAsync(string key, CancellationToken cancellationToken)
    {
        return _repository.DeleteAsync(HashKey(key), cancellationToken);
    }

    /// <summary>
    /// Серия забывается, если после последней попытки или конца блокировки (что позже) прошло окно без попыток.
    /// Окно считается от конца блокировки, иначе при задержке не меньше окна серия обнулялась бы сразу после неё
    /// </summary>
    private LoginAttemptState? Actual(LoginAttemptState? state, DateTimeOffset now)
    {
        if (state is null)
        {
            return null;
        }

        var quietSince = state.LockedUntilUtc > state.LastAttemptAtUtc
            ? state.LockedUntilUtc.Value
            : state.LastAttemptAtUtc;

        return now - quietSince >= _policy.Window ? null : state;
    }

    private TimeSpan DelayFor(int failedCount)
    {
        var doublings = Math.Min(failedCount - _policy.MaxAttempts, MaxDoublings);
        var seconds = _policy.BaseDelay.TotalSeconds * Math.Pow(2, doublings);

        return TimeSpan.FromSeconds(Math.Min(seconds, _policy.MaxDelay.TotalSeconds));
    }

    private static int SecondsUntil(DateTimeOffset moment, DateTimeOffset now)
    {
        return Math.Max(1, (int)Math.Ceiling((moment - now).TotalSeconds));
    }

    /// <summary>
    /// В базу уходит хэш ключа: фиксированная длина, почты не хранятся в открытом виде
    /// </summary>
    private static string HashKey(string key)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
    }
}
