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
    private readonly IQuestionRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly INotificationPushService _notificationPush;
    private readonly INotificationRepository _notificationRepository;

    public QuestionService(
        IQuestionRepository repository,
        IJwtService jwtService,
        INotificationPushService notificationPush,
        INotificationRepository notificationRepository)
    {
        _repository = repository;
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

    public async Task ReplyToQuestionAsync(Guid questionId, ReplyToQuestionRequest request, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var (ok, studentId) = await _repository.ReplyToQuestionAsync(questionId, user.UserId, IsAdmin(user), request, ct);
        if (!ok) throw new QuestionException("question.not_found", "Вопрос не найден");

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

    public async Task SendStudentMessageAsync(Guid questionId, ReplyToQuestionRequest request, CancellationToken ct)
    {
        var user = _jwtService.GetCurrentUser();
        var ok = await _repository.SendStudentMessageAsync(questionId, user.UserId, request.Content, ct);
        if (!ok) throw new QuestionException("question.not_found", "Вопрос не найден или уже закрыт");

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
