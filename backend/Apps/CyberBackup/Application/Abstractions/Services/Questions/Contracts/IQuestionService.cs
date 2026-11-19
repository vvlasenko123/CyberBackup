using Application.DTO.Laboratories;
using Application.DTO.Questions;

namespace Application.Abstractions.Services.Questions.Contracts;

/// <summary>
/// Сервис вопросов
/// </summary>
public interface IQuestionService
{
    Task<IReadOnlyCollection<QuestionListItemDto>> GetMyQuestionsAsync(CancellationToken ct);
    Task<QuestionDetailDto> GetMyQuestionDetailAsync(Guid questionId, CancellationToken ct);
    Task<Guid> CreateQuestionAsync(CreateQuestionRequest request, CancellationToken ct); // уведомляет преподавателей
    Task CloseMyQuestionAsync(Guid questionId, CancellationToken ct);

    Task<PagedResultDto<TeacherQuestionListItemDto>> GetTeacherQuestionsAsync(GetTeacherQuestionsRequest request, CancellationToken ct);
    Task<QuestionDetailDto> GetTeacherQuestionDetailAsync(Guid questionId, CancellationToken ct);
    Task ReplyToQuestionAsync(Guid questionId, ReplyToQuestionRequest request, CancellationToken ct, IReadOnlyList<UploadQuestionAttachmentDto>? files = null);
    Task SendStudentMessageAsync(Guid questionId, ReplyToQuestionRequest request, CancellationToken ct, IReadOnlyList<UploadQuestionAttachmentDto>? files = null);
    Task CloseQuestionByTeacherAsync(Guid questionId, CancellationToken ct);
    Task<QuestionAttachmentDownloadDto> DownloadAttachmentAsync(Guid questionId, Guid attachmentId, CancellationToken ct);
    Task<IReadOnlyCollection<string>> GetLaboratoryTitlesAsync(CancellationToken ct);
}
