using Application.Abstractions.Services.Auth;
using Application.Abstractions.Services.Auth.Contracts;
using Tests.Support;

namespace Tests;

/// <summary>
/// Лимит попыток и нарастающая задержка: 5 попыток, блокировка 30 с, 60 с, 120 с, ... до 15 минут
/// </summary>
public sealed class PasswordAttemptLimiterTests
{
    private readonly ManualTimeProvider _time = new();
    private readonly InMemoryLoginAttemptRepository _repository = new();
    private readonly PasswordAttemptLimiter _limiter;

    public PasswordAttemptLimiterTests()
    {
        _limiter = new PasswordAttemptLimiter(_repository, _time, PasswordAttemptPolicy.Default);
    }

    [Fact]
    public async Task Remaining_attempts_decrease_and_fifth_attempt_starts_the_lock()
    {
        for (var expected = 4; expected >= 1; expected--)
        {
            var attempt = await _limiter.BeginAttemptAsync("k", default);

            Assert.True(attempt.IsAllowed);
            Assert.Equal(expected, attempt.RemainingAttempts);
            Assert.Null(attempt.LockedUntilUtc);
            Assert.Equal(0, attempt.RetryAfterSeconds);
        }

        var fifth = await _limiter.BeginAttemptAsync("k", default);

        Assert.True(fifth.IsAllowed);
        Assert.Equal(0, fifth.RemainingAttempts);
        Assert.Equal(_time.GetUtcNow().AddSeconds(30), fifth.LockedUntilUtc);
        Assert.Equal(30, fifth.RetryAfterSeconds);
    }

    [Fact]
    public async Task Attempts_during_the_lock_are_rejected_and_do_not_extend_it()
    {
        await Exhaust("k");
        var lockedUntil = _time.GetUtcNow().AddSeconds(30);

        _time.Advance(TimeSpan.FromSeconds(10));
        var rejected = await _limiter.BeginAttemptAsync("k", default);

        Assert.False(rejected.IsAllowed);
        Assert.Equal(0, rejected.RemainingAttempts);
        Assert.Equal(lockedUntil, rejected.LockedUntilUtc);
        Assert.Equal(20, rejected.RetryAfterSeconds);

        _time.Advance(TimeSpan.FromSeconds(20));

        Assert.True((await _limiter.BeginAttemptAsync("k", default)).IsAllowed);
    }

    [Fact]
    public async Task Each_attempt_after_the_lock_doubles_the_delay()
    {
        var attempt = await Exhaust("k");
        var delays = new List<int> { attempt.RetryAfterSeconds };

        for (var i = 0; i < 4; i++)
        {
            _time.Advance(TimeSpan.FromSeconds(attempt.RetryAfterSeconds));
            attempt = await _limiter.BeginAttemptAsync("k", default);

            Assert.True(attempt.IsAllowed);
            delays.Add(attempt.RetryAfterSeconds);
        }

        Assert.Equal([30, 60, 120, 240, 480], delays);
    }

    [Fact]
    public async Task Delay_is_capped_and_the_series_survives_the_longest_lock()
    {
        var attempt = await Exhaust("k");

        for (var i = 0; i < 12; i++)
        {
            _time.Advance(TimeSpan.FromSeconds(attempt.RetryAfterSeconds));
            attempt = await _limiter.BeginAttemptAsync("k", default);

            // Если бы серия обнулялась сразу после блокировки, задержка снова стала бы 30 с
            Assert.True(attempt.IsAllowed);
            Assert.Equal(0, attempt.RemainingAttempts);
        }

        Assert.Equal(15 * 60, attempt.RetryAfterSeconds);
    }

    [Fact]
    public async Task Series_is_forgotten_after_a_quiet_window_following_the_lock()
    {
        var attempt = await Exhaust("k");

        _time.Advance(TimeSpan.FromSeconds(attempt.RetryAfterSeconds) + TimeSpan.FromMinutes(15));
        attempt = await _limiter.BeginAttemptAsync("k", default);

        Assert.Equal(4, attempt.RemainingAttempts);
        Assert.Null(attempt.LockedUntilUtc);
    }

    [Fact]
    public async Task Reset_forgets_the_failures()
    {
        await _limiter.BeginAttemptAsync("k", default);
        await _limiter.BeginAttemptAsync("k", default);

        await _limiter.ResetAsync("k", default);

        var attempt = await _limiter.BeginAttemptAsync("k", default);
        Assert.Equal(4, attempt.RemainingAttempts);
    }

    [Fact]
    public async Task Series_is_forgotten_after_the_window_without_attempts()
    {
        await _limiter.BeginAttemptAsync("k", default);
        await _limiter.BeginAttemptAsync("k", default);
        await _limiter.BeginAttemptAsync("k", default);

        _time.Advance(TimeSpan.FromMinutes(15));

        var attempt = await _limiter.BeginAttemptAsync("k", default);
        Assert.Equal(4, attempt.RemainingAttempts);
    }

    [Fact]
    public async Task Series_is_not_forgotten_inside_the_window()
    {
        await _limiter.BeginAttemptAsync("k", default);
        _time.Advance(TimeSpan.FromMinutes(14));
        await _limiter.BeginAttemptAsync("k", default);
        _time.Advance(TimeSpan.FromMinutes(14));

        var attempt = await _limiter.BeginAttemptAsync("k", default);

        Assert.Equal(2, attempt.RemainingAttempts);
    }

    [Fact]
    public async Task Parallel_attempts_cannot_exceed_the_limit()
    {
        var results = await Task.WhenAll(Enumerable.Range(0, 50)
            .Select(_ => Task.Run(() => _limiter.BeginAttemptAsync("k", default))));

        // Пять попыток проходят и включают блокировку, остальные отклоняются ещё до проверки пароля
        Assert.Equal(5, results.Count(x => x.IsAllowed));
        Assert.Equal(45, results.Count(x => !x.IsAllowed));
    }

    [Fact]
    public async Task Keys_are_independent()
    {
        await Exhaust("first");

        var other = await _limiter.BeginAttemptAsync("second", default);

        Assert.True(other.IsAllowed);
        Assert.Equal(4, other.RemainingAttempts);
    }

    [Fact]
    public async Task Storage_receives_a_hash_instead_of_the_key()
    {
        await _limiter.BeginAttemptAsync("login:student@example.com", default);

        var stored = Assert.Single(_repository.Keys);
        Assert.DoesNotContain("student", stored);
        Assert.Equal(64, stored.Length);
    }

    /// <summary>
    /// Пять попыток подряд; возвращает результат пятой, которая включила блокировку
    /// </summary>
    private async Task<PasswordAttemptResult> Exhaust(string key)
    {
        PasswordAttemptResult? last = null;

        for (var i = 0; i < 5; i++)
        {
            last = await _limiter.BeginAttemptAsync(key, default);
        }

        return last!;
    }
}
