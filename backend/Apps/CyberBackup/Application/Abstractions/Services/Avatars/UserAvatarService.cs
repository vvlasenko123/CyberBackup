using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services.Avatars.Contracts;
using Application.DTO.Avatars;
using Domain.Repositories;
using Domain.User;
using Microsoft.Extensions.Logging;

namespace Application.Abstractions.Services.Avatars;

/// <inheritdoc />
public sealed class UserAvatarService : IUserAvatarService
{
    private const string AvatarsPrefix = "user-avatars";

    /// <summary>
    /// Декодированное изображение 4096x4096 занимает порядка 64 МБ, поэтому обработку ограничиваем
    /// </summary>
    private static readonly SemaphoreSlim ProcessingGate = new(2, 2);

    private readonly IUserRepository _userRepository;
    private readonly IUserAvatarStorage _storage;
    private readonly IAvatarImageProcessor _imageProcessor;
    private readonly ILogger<UserAvatarService> _logger;

    public UserAvatarService(
        IUserRepository userRepository,
        IUserAvatarStorage storage,
        IAvatarImageProcessor imageProcessor,
        ILogger<UserAvatarService> logger)
    {
        _userRepository = userRepository;
        _storage = storage;
        _imageProcessor = imageProcessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<DateTimeOffset> SetAsync(Guid userId, AvatarUpload? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length <= 0)
        {
            throw new AvatarException(AvatarException.FileRequired, "Файл не передан");
        }

        if (file.Length > AvatarLimits.MaxBytes)
        {
            throw new AvatarException(AvatarException.TooLarge, "Размер файла не должен превышать 1 МБ");
        }

        var content = await ReadBoundedAsync(file.Content, cancellationToken);
        var processed = await ProcessAsync(content, file, cancellationToken);

        var key = $"{AvatarsPrefix}/{userId:N}/{UUIDNext.Uuid.NewSequential():N}.{processed.Extension}";
        var updatedAt = TruncateToMicroseconds(DateTimeOffset.UtcNow);

        // 1. Сначала новый объект под новым уникальным ключом. Сбой хранилища - 500, база не менялась
        await using (var stream = new MemoryStream(processed.Bytes, writable: false))
        {
            await _storage.SaveAsync(key, stream, processed.Bytes.Length, processed.ContentType, cancellationToken);
        }

        // 2. Затем запись в базу под блокировкой строки, которая возвращает вытесненный ключ
        AvatarChangeResult change;

        try
        {
            change = await _userRepository.SetAvatarAsync(userId, key, updatedAt, cancellationToken);
        }
        catch (AvatarWriteOutcomeUnknownException exception)
        {
            // Запись могла завершиться и база уже ссылается на новый объект, поэтому его не удаляем
            _logger.LogError(
                exception,
                "Исход записи аватара пользователя {UserId} неизвестен, объект {AvatarKey} не удалён",
                userId,
                key);

            throw;
        }
        catch
        {
            // Откат подтверждён, прежний аватар не тронут
            await DiscardObjectAsync(key, CancellationToken.None);

            throw;
        }

        if (!change.UserFound)
        {
            await DiscardObjectAsync(key, CancellationToken.None);

            throw new KeyNotFoundException("Пользователь не найден");
        }

        // 3. Только после записи удаляем вытесненный объект
        await DiscardObjectAsync(change.PreviousPath, CancellationToken.None);

        return updatedAt;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(Guid userId, CancellationToken cancellationToken)
    {
        var change = await _userRepository.ClearAvatarAsync(userId, cancellationToken);

        if (!change.UserFound)
        {
            throw new KeyNotFoundException("Пользователь не найден");
        }

        await DiscardObjectAsync(change.PreviousPath, CancellationToken.None);
    }

    /// <inheritdoc />
    public async Task<AvatarDownload> GetAsync(Guid userId, string? ifNoneMatch, CancellationToken cancellationToken)
    {
        var state = await _userRepository.GetAvatarStateAsync(userId, cancellationToken);

        if (state is null)
        {
            throw new KeyNotFoundException("Пользователь не найден");
        }

        if (state.Path is null)
        {
            throw new KeyNotFoundException("Аватар не найден");
        }

        var etag = BuildETag(state.Path);

        if (MatchesETag(ifNoneMatch, etag))
        {
            return new AvatarDownload(etag, NotModified: true, ContentType: null, Content: null);
        }

        var content = await _storage.OpenReadAsync(state.Path, cancellationToken);

        if (content is null)
        {
            _logger.LogWarning(
                "Для пользователя {UserId} в базе есть ключ аватара {AvatarKey}, но объекта нет в хранилище",
                userId,
                state.Path);

            throw new KeyNotFoundException("Аватар не найден");
        }

        return new AvatarDownload(etag, NotModified: false, ContentTypeFromKey(state.Path), content);
    }

    /// <inheritdoc />
    public async Task DiscardObjectAsync(string? key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        try
        {
            await _storage.DeleteAsync(key, cancellationToken);
        }
        catch (Exception exception)
        {
            // Изменение в базе уже выполнено, объект останется «осиротевшим»
            _logger.LogWarning(exception, "Не удалось удалить объект аватара {AvatarKey}", key);
        }
    }

    private async Task<ProcessedAvatar> ProcessAsync(byte[] content, AvatarUpload file, CancellationToken cancellationToken)
    {
        await ProcessingGate.WaitAsync(cancellationToken);

        try
        {
            return await Task.Run(
                () => _imageProcessor.Process(content, file.FileName, file.ContentType),
                cancellationToken);
        }
        finally
        {
            ProcessingGate.Release();
        }
    }

    /// <summary>
    /// Читает не больше лимита: заявленный Length не считаем достоверным
    /// </summary>
    private static async Task<byte[]> ReadBoundedAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        var chunk = new byte[16 * 1024];

        while (true)
        {
            var read = await stream.ReadAsync(chunk, cancellationToken);

            if (read == 0)
            {
                break;
            }

            buffer.Write(chunk, 0, read);

            if (buffer.Length > AvatarLimits.MaxBytes)
            {
                throw new AvatarException(AvatarException.TooLarge, "Размер файла не должен превышать 1 МБ");
            }
        }

        if (buffer.Length == 0)
        {
            throw new AvatarException(AvatarException.FileRequired, "Файл не передан");
        }

        return buffer.ToArray();
    }

    /// <summary>
    /// Непрозрачный ETag, однозначно связанный с уникальным ключом объекта
    /// </summary>
    internal static string BuildETag(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var token = Convert.ToBase64String(hash)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        return $"\"{token}\"";
    }

    internal static bool MatchesETag(string? ifNoneMatch, string etag)
    {
        if (string.IsNullOrWhiteSpace(ifNoneMatch))
        {
            return false;
        }

        foreach (var raw in ifNoneMatch.Split(','))
        {
            var candidate = raw.Trim();

            if (candidate == "*")
            {
                return true;
            }

            if (candidate.StartsWith("W/", StringComparison.Ordinal))
            {
                candidate = candidate[2..];
            }

            if (candidate == etag)
            {
                return true;
            }
        }

        return false;
    }

    private static string ContentTypeFromKey(string key)
    {
        return Path.GetExtension(key).ToLowerInvariant() switch
        {
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    private static DateTimeOffset TruncateToMicroseconds(DateTimeOffset value)
    {
        return new DateTimeOffset(value.Ticks - value.Ticks % 10, TimeSpan.Zero);
    }
}
