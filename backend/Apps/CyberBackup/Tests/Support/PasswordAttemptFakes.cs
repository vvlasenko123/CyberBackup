using Application.Abstractions.Services.Auth.Contracts;
using Application.DTO.Auth;
using Domain.Auth;
using Domain.Repositories;
using Domain.User;
using Domain.User.Enums;
using Domain.User.ValueObjects;

namespace Tests.Support;

/// <summary>
/// Часы, которыми управляет тест
/// </summary>
internal sealed class ManualTimeProvider : TimeProvider
{
    private DateTimeOffset _now = new(2026, 11, 21, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}

/// <summary>
/// Счётчики попыток в памяти. Блокировка строки в Postgres (FOR UPDATE) моделируется lock-ом
/// </summary>
internal sealed class InMemoryLoginAttemptRepository : ILoginAttemptRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<string, LoginAttemptState> _states = new();

    public IReadOnlyCollection<string> Keys
    {
        get
        {
            lock (_gate)
            {
                return _states.Keys.ToList();
            }
        }
    }

    public Task UpdateAsync(
        string key,
        Func<LoginAttemptState?, LoginAttemptState> update,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _states[key] = update(_states.GetValueOrDefault(key));
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            _states.Remove(key);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Пользователи для сценариев входа и смены пароля; всё, кроме поиска и смены пароля, не используется
/// </summary>
internal sealed class CredentialsUserRepository : IUserRepository
{
    private readonly Dictionary<string, UserModel> _byEmail = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, UserModel> _byId = new();

    public int ChangePasswordCalls { get; private set; }

    public UserModel Add(string email, string passwordHash, bool isActive = true)
    {
        var user = new UserModel(
            Guid.NewGuid(),
            new Email(email),
            new FullName("Иван Иванов"),
            new PasswordHash(passwordHash),
            UserRole.Student,
            isActive,
            false,
            null,
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow,
            null,
            null,
            null,
            null,
            null);

        _byEmail[email] = user;
        _byId[user.Id] = user;

        return user;
    }

    public Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_byEmail.GetValueOrDefault(email));

    public Task<UserModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(_byId.GetValueOrDefault(id));

    public Task ChangePasswordAsync(Guid userId, PasswordHash passwordHash, CancellationToken cancellationToken)
    {
        ChangePasswordCalls++;

        return Task.CompletedTask;
    }

    public Task CreateUserAsync(UserModel userModel, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task UpdateUserAsync(UserModel userModel, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<string?> DeleteUserAsync(Guid id, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<UserAvatarState?> GetAvatarStateAsync(Guid userId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<AvatarChangeResult> SetAvatarAsync(
        Guid userId,
        string avatarPath,
        DateTimeOffset avatarUpdatedAt,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<AvatarChangeResult> ClearAvatarAsync(Guid userId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> UpdateContactsAsync(
        Guid userId,
        PhoneNumber? phone,
        string? telegram,
        string? vk,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<IReadOnlyCollection<UserModel>> GetAllAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> ExistsByEmailForAnotherUserAsync(Guid userId, string email, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<IReadOnlyCollection<Guid>> GetStudentIdsByTeacherAsync(Guid teacherId, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<IReadOnlyCollection<Guid>> GetUserIdsByRolesAsync(IEnumerable<int> roles, CancellationToken cancellationToken) => throw new NotSupportedException();
}

/// <summary>
/// "Хэш" — сам пароль с префиксом; считает вызовы проверки, чтобы убедиться, что при блокировке пароль не проверяется
/// </summary>
internal sealed class CountingPasswordHashService : IPasswordHashService
{
    public int VerifyCalls { get; private set; }

    public string Hash(string password) => "hash:" + password;

    public bool Verify(string password, string passwordHash)
    {
        VerifyCalls++;

        return passwordHash == "hash:" + password;
    }
}

internal sealed class StubJwtService : IJwtService
{
    public Guid CurrentUserId { get; set; }

    public GeneratedAccessTokenDto GenerateAccessToken(TokenUserDataDto userData) =>
        new("access", "jti", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(15));

    public CurrentTokenUserDto GetCurrentUser() => new(CurrentUserId, UserRole.Student);
}

internal sealed class StubRefreshTokenService : IRefreshTokenService
{
    public GeneratedRefreshTokenDto GenerateRefreshToken(DateTimeOffset createdAtUtc) =>
        new("refresh", "refresh-hash", createdAtUtc.AddDays(7));

    public string Hash(string refreshToken) => "refresh-hash";
}

internal sealed class StubAuthTokenDefaults : IAuthTokenDefaultsService
{
    public string ClientId => "tests";

    public IReadOnlyCollection<string> Scopes => ["identity"];
}

internal sealed class StubRefreshTokenRepository : IRefreshTokenRepository
{
    public Task CreateRefreshTokenAsync(RefreshTokenModel refreshTokenModel, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
