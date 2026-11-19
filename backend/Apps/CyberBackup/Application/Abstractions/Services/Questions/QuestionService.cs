using Microsoft.Extensions.Logging;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.Services.Questions.Contracts;
using Application.DTO.Calendar;
using Application.DTO.Laboratories;
using Application.DTO.Questions;
using Domain.Calendar;
using Domain.Repositories;
using Domain.User.Enums;

namespace Application.Abstractions.Services.Questions;

/// <inheritdoc />
public sealed class QuestionService : IQuestionService
{
    private readonly ILogger<QuestionService> _logger;
    private readonly IQuestionAttachmentStorage _attachmentStorage;
    private readonly IQuestionRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly INotificationPushService _notificationPush;
    private readonly INotificationRepository _notificationRepository;

    public QuestionService(
        IQuestionRepository repository,
        IQuestionAttachmentStorage attachmentStorage,
        ILogger<QuestionService> logger,
        IJwtService jwtService,
        INotificationPushService notificationPush,
        INotificationRepository notificationRepository)
    {
        _repository = repository;
        _attachmentStorage = attachmentStorage;
        _logger = logger;
        _jwtService = jwtService;
        _notificationPush = notificationPush;
        _notificationRepository = notificationRepository;
    }

    public Task<IReadOnlyCollection<QuestionListItemDto>> GetMyQuestionsAsync(CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        return _repository.GetMyQuestionsAsync(user.UserId, ct);
    }

    public async Task<QuestionDetailDto> GetMyQuestionDetailAsync(Guid questionId, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var detail = await _repository.GetQuestionDetailAsync(questionId, user.UserId, ct);
        if (detail is null) throw new QuestionException("question.not_found", "Вопрос не найден");
        return detail;
    }

    public async Task<Guid> CreateQuestionAsync(CreateQuestionRequest request, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var questionId = await _repository.CreateQuestionAsync(user.UserId, request, ct);

        // Уведомляем всех преподавателей групп этого студента
        await NotifyTeachersAsync(
            studentId: user.UserId,
            title: "Новый вопрос",
            message: $"Студент задал новый вопрос{(request.LaboratoryTitle is not null ? $" по теме «{request.LaboratoryTitle}»" : "")}",
            ct);

        return questionId;
    }

    public async Task CloseMyQuestionAsync(Guid questionId, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var ok = await _repository.CloseQuestionAsync(questionId, user.UserId, ct);
        if (!ok) throw new QuestionException("question.not_found", "Вопрос не найден");
    }

    public Task<PagedResultDto<TeacherQuestionListItemDto>> GetTeacherQuestionsAsync(
        GetTeacherQuestionsRequest request, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var normalized = request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 100)
        };
        return _repository.GetTeacherQuestionsAsync(normalized, user.UserId, IsAdmin(user), ct);
    }

    public async Task<QuestionDetailDto> GetTeacherQuestionDetailAsync(Guid questionId, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var detail = await _repository.GetQuestionDetailForTeacherAsync(questionId, user.UserId, IsAdmin(user), ct);
        if (detail is null) throw new QuestionException("question.not_found", "Вопрос не найден");
        return detail;
    }

    public async Task ReplyToQuestionAsync(Guid questionId, ReplyToQuestionRequest request, CancellationToken ct, IReadOnlyList<UploadQuestionAttachmentDto>? files = null)
    {
        var user = _jwtService.GetCurrentUser();
        request = await PrepareMessageAsync(questionId, request, files, ct);
        Guid studentId;
        try
        {
            var result = await _repository.ReplyToQuestionAsync(questionId, user.UserId, IsAdmin(user), request, ct);
            if (!result.Success) throw new QuestionException("question.not_found", "Вопрос не найден или уже закрыт");
            studentId = result.StudentId;
        }
        catch
        {
            await DeleteAttachmentsAsync(request.Attachments);
            throw;
        }

        // Уведомляем студента об ответе и сохраняем в БД
        if (studentId != Guid.Empty)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            const string title = "Ответ на вопрос";
            const string message = "Преподаватель ответил на ваш вопрос";
            var notification = new NotificationModel(
                id: UUIDNext.Uuid.NewSequential(),
                userId: studentId,
                calendarEventId: null,
                title: title,
                message: message,
                isRead: false,
                createdAtUtc: nowUtc);

            await _notificationRepository.CreateAsync(notification, ct);
            await _notificationPush.SendToUserAsync(
                studentId,
                new NotificationMessageDto(notification.Id, title, message, nowUtc),
                ct);
        }
    }

    public async Task SendStudentMessageAsync(Guid questionId, ReplyToQuestionRequest request, CancellationToken ct, IReadOnlyList<UploadQuestionAttachmentDto>? files = null)
    {
        var user = _jwtService.GetCurrentUser();
        request = await PrepareMessageAsync(questionId, request, files, ct);
        try
        {
            var ok = await _repository.SendStudentMessageAsync(questionId, user.UserId, request, ct);
            if (!ok) throw new QuestionException("question.not_found", "Вопрос не найден или уже закрыт");
        }
        catch
        {
            await DeleteAttachmentsAsync(request.Attachments);
            throw;
        }

        // Уведомляем преподавателей о новом сообщении студента
        await NotifyTeachersAsync(
            studentId: user.UserId,
            title: "Новое сообщение",
            message: "Студент написал новое сообщение в вопросе",
            ct);
    }

    public async Task CloseQuestionByTeacherAsync(Guid questionId, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var ok = await _repository.CloseQuestionByTeacherAsync(questionId, user.UserId, IsAdmin(user), ct);
        if (!ok) throw new QuestionException("question.not_found", "Вопрос не найден");
    }

    public Task<IReadOnlyCollection<string>> GetLaboratoryTitlesAsync(CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        return _repository.GetLaboratoryTitlesAsync(user.UserId, IsAdmin(user), ct);
    }

    public async Task<QuestionAttachmentDownloadDto> DownloadAttachmentAsync(Guid questionId, Guid attachmentId, CancellationToken ct)
    {
        var detail = await GetAccessibleQuestionAsync(questionId, ct);
        var attachment = detail.Messages.SelectMany(m => m.Attachments).FirstOrDefault(a => a.Id == attachmentId)
            ?? throw new QuestionException("question.attachment_not_found", "Вложение не найдено");
        var stream = await _attachmentStorage.OpenReadAsync(attachment.StoragePath, ct);
        return new(stream, attachment.FileName, attachment.ContentType);
    }

    private Task<QuestionDetailDto> GetAccessibleQuestionAsync(Guid questionId, CancellationToken ct)
        => _jwtService.GetCurrentUser().Role == UserRole.Student
            ? GetMyQuestionDetailAsync(questionId, ct)
            : GetTeacherQuestionDetailAsync(questionId, ct);

    private async Task<ReplyToQuestionRequest> PrepareMessageAsync(Guid questionId, ReplyToQuestionRequest request,
        IReadOnlyList<UploadQuestionAttachmentDto>? files, CancellationToken ct)
    {
        files ??= [];
        var content = request.Content?.Trim() ?? string.Empty;
        if ((content.Length == 0 && files.Count == 0) || content.Length > 3000 || files.Count > 5)
            throw new QuestionException("question.invalid_message", "Добавьте текст до 3000 символов или до 5 файлов");
        var detail = await GetAccessibleQuestionAsync(questionId, ct);
        if (detail.Status == Domain.Questions.Enums.QuestionStatus.Closed)
            throw new QuestionException("question.closed", "Вопрос уже закрыт");

        var validated = new List<QuestionAttachmentDto>();
        foreach (var file in files)
        {
            var name = Path.GetFileName(file.FileName.Replace('\\', '/'));
            var ext = Path.GetExtension(name).ToLowerInvariant();
            var mime = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png",
                ".gif" => "image/gif", ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => throw new QuestionException("question.invalid_file", "Допустимы JPG, PNG, GIF, PDF и DOCX")
            };
            if (file.Length <= 0 || file.Length > 10 * 1024 * 1024 || name.Length > 255 || !file.Content.CanSeek)
                throw new QuestionException("question.invalid_file", "Файл должен быть непустым, до 10 МБ, имя — до 255 символов");
            var header = new byte[12];
            file.Content.Position = 0;
            var count = await file.Content.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false, cancellationToken: ct);
            file.Content.Position = 0;
            var valid = ext switch
            {
                ".jpg" or ".jpeg" => count >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255,
                ".png" => count >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}),
                ".gif" => count >= 6 && (header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8)),
                ".pdf" => count >= 5 && header.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
                ".docx" => IsDocx(file.Content),
                _ => false
            };
            file.Content.Position = 0;
            if (!valid) throw new QuestionException("question.invalid_file", "Содержимое файла не соответствует его формату");
            var id = UUIDNext.Uuid.NewSequential();
            validated.Add(new QuestionAttachmentDto { Id = id, FileName = name, ContentType = mime,
                SizeBytes = file.Length, StoragePath = $"question-attachments/{questionId:N}/{id:N}{ext}" });
        }
        var uploaded = new List<QuestionAttachmentDto>();
        try
        {
            for (var i = 0; i < validated.Count; i++)
            {
                var attachment = validated[i];
                uploaded.Add(attachment);
                await _attachmentStorage.SaveAsync(attachment.StoragePath, files[i].Content, files[i].Length, attachment.ContentType, ct);
            }
        }
        catch
        {
            await DeleteAttachmentsAsync(uploaded);
            throw;
        }
        return new ReplyToQuestionRequest { Content = content, Attachments = validated };
    }

    private static bool IsDocx(Stream stream)
    {
        try
        {
            using var zip = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
            return zip.GetEntry("[Content_Types].xml") != null && zip.GetEntry("word/document.xml") != null;
        }
        catch (InvalidDataException) { return false; }
    }

    private async Task DeleteAttachmentsAsync(IEnumerable<QuestionAttachmentDto> attachments)
    {
        foreach (var attachment in attachments)
        {
            // Cleanup must not replace the original upload/database error.
            try { await _attachmentStorage.DeleteAsync(attachment.StoragePath, CancellationToken.None); }
            catch (Exception ex) { _logger.LogWarning(ex, "Failed to remove uncommitted question attachment {StoragePath}", attachment.StoragePath); }
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Отправить уведомление всем преподавателям групп студента и сохранить в БД</summary>
    private async Task NotifyTeachersAsync(Guid studentId, string title, string message, CancellationToken ct)
    {
        var teacherIds = await _repository.GetTeacherIdsForStudentAsync(studentId, ct);
        var now = DateTimeOffset.UtcNow;

        foreach (var teacherId in teacherIds)
        {
            var notification = new NotificationModel(
                id: UUIDNext.Uuid.NewSequential(),
                userId: teacherId,
                calendarEventId: null,
                title: title,
                message: message,
                isRead: false,
                createdAtUtc: now);

            await _notificationRepository.CreateAsync(notification, ct);
            await _notificationPush.SendToUserAsync(
                teacherId,
                new NotificationMessageDto(notification.Id, title, message, now),
                ct);
        }
    }

    private static bool IsAdmin(Application.DTO.Auth.CurrentTokenUserDto u) =>
        u.Role is UserRole.Admin or UserRole.SuperAdmin;
}
