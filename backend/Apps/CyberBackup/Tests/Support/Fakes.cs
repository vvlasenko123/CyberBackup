using System.Collections.Concurrent;
using Application.Abstractions.Services.Avatars.Contracts;
using Application.DTO.Avatars;
using Domain.Repositories;
using Domain.User;
using Microsoft.Extensions.Logging;

namespace Tests.Support;

/// <summary>
/// Режимы сбоя записи аватара в базу
/// </summary>
public enum DbFailure
{
    None,

    /// <summary>
    /// Ошибка до коммита: транзакция откатилась, ничего не записано
    /// </summary>
    BeforeCommit,

    /// <summary>
    /// Обрыв при коммите: запись применилась, но клиент об этом не узнал
    /// </summary>
    UnknownAfterApplied,

    /// <summary>
    /// Обрыв при коммите: запись не применилась, но клиент об этом тоже не узнал
    /// </summary>
    UnknownNotApplied
}

/// <summary>
/// Репозиторий пользователей в памяти. Блокировка строки моделируется lock-ом
/// </summary>
internal sealed class FakeUserRepository : IUserRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, UserRow> _users = new();

    public DbFailure Failure { get; set; }

    public Action? BeforeSetCommit { get; set; }

    public int GetAvatarStateCalls { get; private set; }

    public Guid AddUser(string? avatarPath = null)
    {
        var id = Guid.NewGuid();

        lock (_gate)
        {
            _users[id] = new UserRow(avatarPath, avatarPath is null ? null : DateTimeOffset.UtcNow);
        }

        return id;
    }

    public void RemoveUser(Guid id)
    {
        lock (_gate)
        {
            _users.Remove(id);
        }
    }

    public string? AvatarPathOf(Guid id)
    {
        lock (_gate)
        {
            return _users[id].Path;
        }
    }

    public Task<string?> DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_users.Remove(id, out var row))
            {
                return Task.FromResult<string?>(null);
            }

            return Task.FromResult(row.Path);
        }
    }

    public Task<UserAvatarState?> GetAvatarStateAsync(Guid userId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            GetAvatarStateCalls++;

            return Task.FromResult(_users.TryGetValue(userId, out var row)
                ? new UserAvatarState(row.Path, row.UpdatedAt)
                : null);
        }
    }

    public Task<AvatarChangeResult> SetAvatarAsync(
        Guid userId,
        string avatarPath,
        DateTimeOffset avatarUpdatedAt,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            BeforeSetCommit?.Invoke();

            if (!_users.TryGetValue(userId, out var row))
            {
                return Task.FromResult(new AvatarChangeResult(false, null));
            }

            switch (Failure)
            {
                case DbFailure.BeforeCommit:
                    throw new InvalidOperationException("сбой базы до коммита");
                case DbFailure.UnknownAfterApplied:
                    _users[userId] = new UserRow(avatarPath, avatarUpdatedAt);
                    throw new AvatarWriteOutcomeUnknownException(new IOException("обрыв при коммите"));
                case DbFailure.UnknownNotApplied:
                    throw new AvatarWriteOutcomeUnknownException(new IOException("обрыв при коммите"));
            }

            _users[userId] = new UserRow(avatarPath, avatarUpdatedAt);

            return Task.FromResult(new AvatarChangeResult(true, row.Path));
        }
    }

    public Task<AvatarChangeResult> ClearAvatarAsync(Guid userId, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_users.TryGetValue(userId, out var row))
            {
                return Task.FromResult(new AvatarChangeResult(false, null));
            }

            if (Failure == DbFailure.BeforeCommit)
            {
                throw new InvalidOperationException("сбой базы до коммита");
            }

            _users[userId] = new UserRow(null, null);

            return Task.FromResult(new AvatarChangeResult(true, row.Path));
        }
    }

    public Task<bool> UpdateContactsAsync(
        Guid userId,
        Domain.User.ValueObjects.PhoneNumber? phone,
        string? telegram,
        string? vk,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_users.TryGetValue(userId, out var row))
            {
                return Task.FromResult(false);
            }

            _users[userId] = row with { Phone = phone, Telegram = telegram, Vk = vk };

            return Task.FromResult(true);
        }
    }

    public (string? Phone, string? Telegram, string? Vk) ContactsOf(Guid id)
    {
        lock (_gate)
        {
            var row = _users[id];

            return (row.Phone?.Value, row.Telegram, row.Vk);
        }
    }

    public Task CreateUserAsync(UserModel userModel, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task UpdateUserAsync(UserModel userModel, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<UserModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            if (!_users.TryGetValue(id, out var row))
            {
                return Task.FromResult<UserModel?>(null);
            }

            return Task.FromResult<UserModel?>(new UserModel(
                id,
                new Domain.User.ValueObjects.Email("user@example.com"),
                new Domain.User.ValueObjects.FullName("Test User"),
                new Domain.User.ValueObjects.PasswordHash("hash"),
                Domain.User.Enums.UserRole.Student,
                isActive: true,
                mustChangePassword: false,
                createdBy: null,
                createdAt: DateTimeOffset.UtcNow,
                updatedAt: DateTimeOffset.UtcNow,
                avatarPath: row.Path,
                avatarUpdatedAt: row.UpdatedAt,
                phone: row.Phone,
                telegram: row.Telegram,
                vk: row.Vk));
        }
    }

    public Task<IReadOnlyCollection<UserModel>> GetAllAsync(CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task<bool> ExistsByEmailForAnotherUserAsync(Guid userId, string email, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken) => throw new NotSupportedException();

    public Task ChangePasswordAsync(Guid userId, Domain.User.ValueObjects.PasswordHash passwordHash, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<IReadOnlyCollection<Guid>> GetStudentIdsByTeacherAsync(Guid teacherId, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public Task<IReadOnlyCollection<Guid>> GetUserIdsByRolesAsync(IEnumerable<int> roles, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    private sealed record UserRow(
        string? Path,
        DateTimeOffset? UpdatedAt,
        Domain.User.ValueObjects.PhoneNumber? Phone = null,
        string? Telegram = null,
        string? Vk = null);
}

/// <summary>
/// Хранилище аватаров в памяти с внедрением сбоев
/// </summary>
internal sealed class FakeAvatarStorage : IUserAvatarStorage
{
    private readonly ConcurrentDictionary<string, (byte[] Content, string ContentType)> _objects = new();

    public bool FailSave { get; set; }

    public bool FailDelete { get; set; }

    public bool FailRead { get; set; }

    public int OpenReadCalls => _openReadCalls;

    private int _openReadCalls;

    public IReadOnlyCollection<string> Keys => _objects.Keys.ToArray();

    public bool Contains(string key) => _objects.ContainsKey(key);

    public (byte[] Content, string ContentType) Get(string key) => _objects[key];

    public void Put(string key, byte[] content, string contentType = "image/png") => _objects[key] = (content, contentType);

    public Task SaveAsync(string key, Stream content, long length, string contentType, CancellationToken cancellationToken)
    {
        if (FailSave)
        {
            throw new IOException("хранилище недоступно при записи");
        }

        using var buffer = new MemoryStream();
        content.CopyTo(buffer);
        _objects[key] = (buffer.ToArray(), contentType);

        return Task.CompletedTask;
    }

    public Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _openReadCalls);

        if (FailRead)
        {
            throw new IOException("хранилище недоступно при чтении");
        }

        return Task.FromResult<Stream?>(_objects.TryGetValue(key, out var value) ? new MemoryStream(value.Content) : null);
    }

    public Task DeleteAsync(string key, CancellationToken cancellationToken)
    {
        if (FailDelete)
        {
            throw new IOException("хранилище недоступно при удалении");
        }

        _objects.TryRemove(key, out _);

        return Task.CompletedTask;
    }
}

/// <summary>
/// Процессор, который не трогает содержимое: быстрые тесты потока замены и очистки
/// </summary>
internal sealed class PassthroughProcessor : IAvatarImageProcessor
{
    public ProcessedAvatar Process(byte[] content, string fileName, string contentType) =>
        new(content, "png", "image/png");
}

/// <summary>
/// Логгер, запоминающий записи
/// </summary>
internal sealed class ListLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        lock (Entries)
        {
            Entries.Add((logLevel, formatter(state, exception)));
        }
    }
}
