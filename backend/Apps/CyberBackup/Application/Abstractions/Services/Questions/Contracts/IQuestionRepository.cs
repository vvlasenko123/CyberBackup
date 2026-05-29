using Application.DTO.Laboratories;
using Application.DTO.Questions;

namespace Application.Abstractions.Services.Questions.Contracts;

/// <summary>
/// Репозиторий вопросов
/// </summary>
public interface IQuestionRepository
{
    /// <summary>Список вопросов студента</summary>
    Task<IReadOnlyCollection<QuestionListItemDto>> GetMyQuestionsAsync(Guid studentId, CancellationToken ct);

    /// <summary>Детали вопроса (студент видит только свой)</summary>
    Task<QuestionDetailDto?> GetQuestionDetailAsync(Guid questionId, Guid requesterId, CancellationToken ct);

    /// <summary>Детали вопроса (преподаватель)</summary>
    Task<QuestionDetailDto?> GetQuestionDetailForTeacherAsync(Guid questionId, Guid teacherId, bool includeAll, CancellationToken ct);

    /// <summary>Создать вопрос</summary>
    Task<Guid> CreateQuestionAsync(Guid studentId, CreateQuestionRequest request, CancellationToken ct);

    /// <summary>Закрыть вопрос (студент закрывает свой)</summary>
    Task<bool> CloseQuestionAsync(Guid questionId, Guid studentId, CancellationToken ct);

    /// <summary>Список вопросов для преподавателя (его группы)</summary>
    Task<PagedResultDto<TeacherQuestionListItemDto>> GetTeacherQuestionsAsync(GetTeacherQuestionsRequest request, Guid teacherId, bool includeAll, CancellationToken ct);

    /// <summary>Добавить сообщение от преподавателя</summary>
    Task<(bool Success, Guid StudentId)> ReplyToQuestionAsync(Guid questionId, Guid teacherId, bool includeAll, ReplyToQuestionRequest request, CancellationToken ct);

    /// <summary>Студент отправляет сообщение в открытый вопрос</summary>
    Task<bool> SendStudentMessageAsync(Guid questionId, Guid studentId, string content, CancellationToken ct);

    /// <summary>Закрыть вопрос (преподаватель)</summary>
    Task<bool> CloseQuestionByTeacherAsync(Guid questionId, Guid teacherId, bool includeAll, CancellationToken ct);

    /// <summary>Список уникальных названий лаб из вопросов (для фильтра)</summary>
    Task<IReadOnlyCollection<string>> GetLaboratoryTitlesAsync(Guid teacherId, bool includeAll, CancellationToken ct);

    /// <summary>ID преподавателей, у которых студент находится в группе</summary>
    Task<IReadOnlyCollection<Guid>> GetTeacherIdsForStudentAsync(Guid studentId, CancellationToken ct);
}
