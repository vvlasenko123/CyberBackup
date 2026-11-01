using Application.Abstractions.Services.Avatars;
using Application.DTO.Avatars;
using Microsoft.Extensions.Logging;
using Tests.Support;

namespace Tests;

public sealed class UserAvatarServiceTests
{
    private readonly FakeUserRepository _repository = new();
    private readonly FakeAvatarStorage _storage = new();
    private readonly ListLogger<UserAvatarService> _logger = new();

    private UserAvatarService CreateService() =>
        new(_repository, _storage, new PassthroughProcessor(), _logger);

    private static AvatarUpload Upload(string content = "image-bytes") =>
        Upload(System.Text.Encoding.UTF8.GetBytes(content));

    private static AvatarUpload Upload(byte[] bytes) =>
        new(new MemoryStream(bytes), "a.png", "image/png", bytes.Length);

    private static string BodyOf(Stream stream)
    {
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }

    // ---------- загрузка и замена (FR-1) ----------

    [Fact]
    public async Task Upload_stores_object_under_a_unique_key_and_updates_the_database()
    {
        var userId = _repository.AddUser();

        var updatedAt = await CreateService().SetAsync(userId, Upload("first"), CancellationToken.None);

        var key = _repository.AvatarPathOf(userId);
        Assert.NotNull(key);
        Assert.StartsWith($"user-avatars/{userId:N}/", key);
        Assert.EndsWith(".png", key);
        Assert.Equal("first", System.Text.Encoding.UTF8.GetString(_storage.Get(key!).Content));
        Assert.Equal("image/png", _storage.Get(key!).ContentType);
        Assert.Equal(TimeSpan.Zero, updatedAt.Offset);
        Assert.Single(_storage.Keys);
    }

    [Fact]
    public async Task Replacement_gets_a_new_key_and_removes_the_superseded_object()
    {
        var userId = _repository.AddUser();
        var service = CreateService();

        await service.SetAsync(userId, Upload("one"), CancellationToken.None);
        var firstKey = _repository.AvatarPathOf(userId)!;

        await service.SetAsync(userId, Upload("two"), CancellationToken.None);
        var secondKey = _repository.AvatarPathOf(userId)!;

        Assert.NotEqual(firstKey, secondKey);
        Assert.False(_storage.Contains(firstKey));
        Assert.True(_storage.Contains(secondKey));
        Assert.Single(_storage.Keys);
    }

    [Fact]
    public async Task Database_failure_before_commit_removes_the_new_object_and_keeps_the_previous_avatar()
    {
        var previousKey = "user-avatars/old/old.png";
        var userId = _repository.AddUser(previousKey);
        _storage.Put(previousKey, [1, 2, 3]);
        _repository.Failure = DbFailure.BeforeCommit;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().SetAsync(userId, Upload(), CancellationToken.None));

        Assert.Equal(previousKey, _repository.AvatarPathOf(userId));
        Assert.Equal([previousKey], _storage.Keys);
    }

    [Theory]
    [InlineData(DbFailure.UnknownAfterApplied)]
    [InlineData(DbFailure.UnknownNotApplied)]
    public async Task Unknown_commit_outcome_never_deletes_the_new_object(DbFailure failure)
    {
        var previousKey = "user-avatars/old/old.png";
        var userId = _repository.AddUser(previousKey);
        _storage.Put(previousKey, [1, 2, 3]);
        _repository.Failure = failure;

        await Assert.ThrowsAsync<Domain.User.AvatarWriteOutcomeUnknownException>(
            () => CreateService().SetAsync(userId, Upload(), CancellationToken.None));

        // Новый объект остаётся: запись могла завершиться и база уже ссылается на него
        Assert.Equal(2, _storage.Keys.Count);
        Assert.Contains(previousKey, _storage.Keys);
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Error);

        if (failure == DbFailure.UnknownAfterApplied)
        {
            Assert.True(_storage.Contains(_repository.AvatarPathOf(userId)!));
        }
    }

    [Fact]
    public async Task User_deleted_during_processing_gives_not_found_and_cleans_up_the_new_object()
    {
        var userId = _repository.AddUser();
        _repository.RemoveUser(userId);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().SetAsync(userId, Upload(), CancellationToken.None));

        Assert.Empty(_storage.Keys);
    }

    [Fact]
    public async Task Storage_failure_on_save_leaves_the_database_untouched()
    {
        var previousKey = "user-avatars/old/old.png";
        var userId = _repository.AddUser(previousKey);
        _storage.Put(previousKey, [1, 2, 3]);
        _storage.FailSave = true;

        await Assert.ThrowsAsync<IOException>(
            () => CreateService().SetAsync(userId, Upload(), CancellationToken.None));

        Assert.Equal(previousKey, _repository.AvatarPathOf(userId));
        Assert.Equal([previousKey], _storage.Keys);
    }

    [Fact]
    public async Task Failure_to_delete_the_superseded_object_is_logged_and_does_not_fail_the_upload()
    {
        var previousKey = "user-avatars/old/old.png";
        var userId = _repository.AddUser(previousKey);
        _storage.Put(previousKey, [1, 2, 3]);
        _storage.FailDelete = true;

        await CreateService().SetAsync(userId, Upload(), CancellationToken.None);

        Assert.NotEqual(previousKey, _repository.AvatarPathOf(userId));
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Failure_to_delete_the_new_object_after_database_error_keeps_the_original_error()
    {
        var userId = _repository.AddUser();
        _repository.Failure = DbFailure.BeforeCommit;
        _storage.FailDelete = true;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().SetAsync(userId, Upload(), CancellationToken.None));
    }

    [Fact]
    public async Task Concurrent_uploads_leave_exactly_the_last_written_object()
    {
        var oldKey = "user-avatars/old/old.png";
        var userId = _repository.AddUser(oldKey);
        _storage.Put(oldKey, [9]);
        var service = CreateService();

        await Task.WhenAll(Enumerable.Range(0, 30)
            .Select(i => Task.Run(() => service.SetAsync(userId, Upload($"upload-{i}"), CancellationToken.None))));

        var winner = _repository.AvatarPathOf(userId)!;
        Assert.Equal([winner], _storage.Keys);
        Assert.StartsWith("upload-", System.Text.Encoding.UTF8.GetString(_storage.Get(winner).Content));
    }

    [Fact]
    public async Task Concurrent_upload_and_removal_never_leave_orphans_when_removal_is_last()
    {
        var userId = _repository.AddUser();
        var service = CreateService();

        await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(i => Task.Run(() => service.SetAsync(userId, Upload($"upload-{i}"), CancellationToken.None))));
        await service.RemoveAsync(userId, CancellationToken.None);

        Assert.Null(_repository.AvatarPathOf(userId));
        Assert.Empty(_storage.Keys);
    }

    // ---------- проверка входных данных ----------

    [Fact]
    public async Task Missing_file_is_rejected()
    {
        var userId = _repository.AddUser();

        var exception = await Assert.ThrowsAsync<AvatarException>(
            () => CreateService().SetAsync(userId, null, CancellationToken.None));

        Assert.Equal(AvatarException.FileRequired, exception.Code);
    }

    [Fact]
    public async Task Empty_file_is_rejected()
    {
        var userId = _repository.AddUser();

        var exception = await Assert.ThrowsAsync<AvatarException>(
            () => CreateService().SetAsync(userId, Upload([]), CancellationToken.None));

        Assert.Equal(AvatarException.FileRequired, exception.Code);
    }

    [Fact]
    public async Task Declared_size_over_the_limit_is_rejected()
    {
        var userId = _repository.AddUser();
        var upload = new AvatarUpload(new MemoryStream([1]), "a.png", "image/png", AvatarLimits.MaxBytes + 1);

        var exception = await Assert.ThrowsAsync<AvatarException>(
            () => CreateService().SetAsync(userId, upload, CancellationToken.None));

        Assert.Equal(AvatarException.TooLarge, exception.Code);
    }

    [Fact]
    public async Task Actual_stream_longer_than_declared_is_rejected()
    {
        var userId = _repository.AddUser();
        var big = new byte[AvatarLimits.MaxBytes + 100];
        var upload = new AvatarUpload(new MemoryStream(big), "a.png", "image/png", 10);

        var exception = await Assert.ThrowsAsync<AvatarException>(
            () => CreateService().SetAsync(userId, upload, CancellationToken.None));

        Assert.Equal(AvatarException.TooLarge, exception.Code);
        Assert.Empty(_storage.Keys);
    }

    [Fact]
    public async Task Rejected_image_keeps_the_previous_avatar_and_stores_nothing()
    {
        var previousKey = "user-avatars/old/old.png";
        var userId = _repository.AddUser(previousKey);
        _storage.Put(previousKey, [1, 2, 3]);

        // Настоящий процессор отклоняет текст как неизображение
        var service = new UserAvatarService(_repository, _storage, new AvatarImageProcessor(), _logger);
        var upload = new AvatarUpload(new MemoryStream("not an image"u8.ToArray()), "a.png", "image/png", 12);

        var exception = await Assert.ThrowsAsync<AvatarException>(
            () => service.SetAsync(userId, upload, CancellationToken.None));

        Assert.Equal(AvatarException.InvalidFormat, exception.Code);
        Assert.Equal(previousKey, _repository.AvatarPathOf(userId));
        Assert.Equal([previousKey], _storage.Keys);
    }

    // ---------- удаление (FR-2, FR-6) ----------

    [Fact]
    public async Task Removal_clears_the_reference_and_deletes_the_object()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);

        await CreateService().RemoveAsync(userId, CancellationToken.None);

        Assert.Null(_repository.AvatarPathOf(userId));
        Assert.Empty(_storage.Keys);
    }

    [Fact]
    public async Task Removal_for_user_without_avatar_is_idempotent()
    {
        var userId = _repository.AddUser();

        await CreateService().RemoveAsync(userId, CancellationToken.None);
        await CreateService().RemoveAsync(userId, CancellationToken.None);

        Assert.Null(_repository.AvatarPathOf(userId));
    }

    [Fact]
    public async Task Removal_for_missing_user_is_not_found()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().RemoveAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Storage_failure_on_removal_is_logged_but_not_returned()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);
        _storage.FailDelete = true;

        await CreateService().RemoveAsync(userId, CancellationToken.None);

        Assert.Null(_repository.AvatarPathOf(userId));
        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Database_failure_on_removal_keeps_the_object()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);
        _repository.Failure = DbFailure.BeforeCommit;

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().RemoveAsync(userId, CancellationToken.None));

        Assert.Equal(key, _repository.AvatarPathOf(userId));
        Assert.True(_storage.Contains(key));
    }

    // ---------- получение (FR-3) ----------

    [Fact]
    public async Task Get_returns_content_type_from_key_extension_and_etag()
    {
        var key = "user-avatars/u/a.webp";
        var userId = _repository.AddUser(key);
        _storage.Put(key, "payload"u8.ToArray(), "image/webp");

        var result = await CreateService().GetAsync(userId, null, CancellationToken.None);

        Assert.False(result.NotModified);
        Assert.Equal("image/webp", result.ContentType);
        Assert.Equal("payload", BodyOf(result.Content!));
        Assert.StartsWith("\"", result.ETag);
        Assert.EndsWith("\"", result.ETag);
        Assert.DoesNotContain("a.webp", result.ETag);
    }

    [Fact]
    public async Task Get_for_missing_user_or_missing_avatar_is_not_found()
    {
        var withoutAvatar = _repository.AddUser();

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().GetAsync(Guid.NewGuid(), null, CancellationToken.None));
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().GetAsync(withoutAvatar, null, CancellationToken.None));
    }

    [Fact]
    public async Task Matching_if_none_match_gives_not_modified_without_reading_the_storage()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);
        var service = CreateService();

        var first = await service.GetAsync(userId, null, CancellationToken.None);
        var readsBefore = _storage.OpenReadCalls;

        var second = await service.GetAsync(userId, first.ETag, CancellationToken.None);

        Assert.True(second.NotModified);
        Assert.Null(second.Content);
        Assert.Equal(first.ETag, second.ETag);
        Assert.Equal(readsBefore, _storage.OpenReadCalls);
    }

    [Theory]
    [InlineData("weak")]
    [InlineData("list")]
    [InlineData("star")]
    public async Task If_none_match_variants_are_recognised(string variant)
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);
        var service = CreateService();
        var etag = (await service.GetAsync(userId, null, CancellationToken.None)).ETag;

        var header = variant switch
        {
            "weak" => $"W/{etag}",
            "list" => $"\"other\", {etag}",
            _ => "*"
        };

        var result = await service.GetAsync(userId, header, CancellationToken.None);

        Assert.True(result.NotModified);
    }

    [Fact]
    public async Task Non_matching_if_none_match_returns_content()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);

        var result = await CreateService().GetAsync(userId, "\"stale\"", CancellationToken.None);

        Assert.False(result.NotModified);
        Assert.NotNull(result.Content);
    }

    [Fact]
    public async Task Etag_changes_after_replacement()
    {
        var userId = _repository.AddUser();
        var service = CreateService();

        await service.SetAsync(userId, Upload("one"), CancellationToken.None);
        var before = (await service.GetAsync(userId, null, CancellationToken.None)).ETag;

        await service.SetAsync(userId, Upload("one"), CancellationToken.None);
        var after = await service.GetAsync(userId, before, CancellationToken.None);

        Assert.False(after.NotModified);
        Assert.NotEqual(before, after.ETag);
    }

    [Fact]
    public async Task Get_when_object_is_missing_in_storage_is_not_found_and_logged()
    {
        var userId = _repository.AddUser("user-avatars/u/gone.png");

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().GetAsync(userId, null, CancellationToken.None));

        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task Get_when_storage_is_unavailable_propagates_the_failure()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);
        _storage.FailRead = true;

        await Assert.ThrowsAsync<IOException>(
            () => CreateService().GetAsync(userId, null, CancellationToken.None));
    }

    // ---------- очистка при удалении пользователя (FR-5) ----------

    [Fact]
    public async Task Deleting_a_user_removes_the_avatar_object_by_the_key_from_the_delete_operation()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);
        var deleteService = new Application.Abstractions.Services.User.DeleteUserService(_repository, CreateService());

        await deleteService.Delete(userId, CancellationToken.None);

        Assert.Empty(_storage.Keys);
    }

    [Fact]
    public async Task Cleanup_failure_does_not_cancel_user_deletion()
    {
        var key = "user-avatars/u/a.png";
        var userId = _repository.AddUser(key);
        _storage.Put(key, [1]);
        _storage.FailDelete = true;
        var deleteService = new Application.Abstractions.Services.User.DeleteUserService(_repository, CreateService());

        await deleteService.Delete(userId, CancellationToken.None);

        Assert.Contains(_logger.Entries, e => e.Level == LogLevel.Warning);
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().RemoveAsync(userId, CancellationToken.None));
    }

    [Fact]
    public async Task Deleting_a_user_without_avatar_touches_no_storage()
    {
        var userId = _repository.AddUser();
        var deleteService = new Application.Abstractions.Services.User.DeleteUserService(_repository, CreateService());

        await deleteService.Delete(userId, CancellationToken.None);

        Assert.Empty(_storage.Keys);
    }
}
