using Application.DTO.Laboratories;

namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Репозиторий лабораторных работ
/// </summary>
public interface ILaboratoryRepository
{
    /// <summary>
    /// Получить список лабораторных работ для студента
    /// </summary>
    Task<PagedResultDto<GetLaboratoryListItemDto>> GetStudentLaboratoriesAsync(
        Guid studentId,
        GetLaboratoryListRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали лабораторной работы для студента
    /// </summary>
    Task<GetLaboratoryDetailsResponse?> GetStudentLaboratoryDetailsAsync(
        Guid studentId,
        Guid laboratoryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Открыть подсказку лабораторной работы
    /// </summary>
    Task<OpenLaboratoryHintResponse?> OpenHintAsync(
        Guid studentId,
        Guid laboratoryId,
        Guid hintId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Сохранить попытку сдачи флага
    /// </summary>
    Task<SubmitLaboratoryFlagResponse> SubmitFlagAttemptAsync(
        Guid studentId,
        Guid laboratoryId,
        string submittedFlagHash,
        string submittedFlagMasked,
        bool isCorrect,
        CancellationToken cancellationToken);

    /// <summary>
    /// Загрузить отчет по лабораторной работе
    /// </summary>
    Task<UploadLaboratoryReportResponse> UploadReportAsync(
        Guid studentId,
        Guid laboratoryId,
        SavedLaboratoryReportFileDto file,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить отчет студента по лабораторной работе
    /// </summary>
    Task<GetMyLaboratoryReportResponse?> GetMyReportAsync(
        Guid studentId,
        Guid laboratoryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить прогресс студента
    /// </summary>
    Task<GetMyProgressResponse> GetMyProgressAsync(
        Guid studentId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить ведомость студента
    /// </summary>
    Task<GetMyGradebookResponse?> GetMyGradebookAsync(
        Guid studentId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить список лабораторных работ для преподавателя
    /// </summary>
    Task<PagedResultDto<TeacherLaboratoryListItemDto>> GetTeacherLaboratoriesAsync(
        GetTeacherLaboratoryListRequest request,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали лабораторной работы для преподавателя
    /// </summary>
    Task<GetTeacherLaboratoryDetailsResponse?> GetTeacherLaboratoryDetailsAsync(
        Guid laboratoryId,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить ожидаемый хэш флага лабораторной работы
    /// </summary>
    Task<string?> GetExpectedFlagHashAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Создать лабораторную работу
    /// </summary>
    Task<Guid> CreateLaboratoryAsync(
        CreateLaboratoryRequest request,
        string? expectedFlagHash,
        Guid teacherId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Обновить лабораторную работу
    /// </summary>
    Task UpdateLaboratoryAsync(
        Guid laboratoryId,
        UpdateLaboratoryRequest request,
        string? expectedFlagHash,
        bool updateFlagHash,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken);

    /// <summary>
    /// Удалить лабораторную работу
    /// </summary>
    Task DeleteLaboratoryAsync(Guid laboratoryId, Guid teacherId, bool includeAll, CancellationToken cancellationToken);

    /// <summary>
    /// Получить список отчетов студентов для преподавателя
    /// </summary>
    Task<PagedResultDto<TeacherReportListItemDto>> GetTeacherReportsAsync(
        GetTeacherReportListRequest request,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали отчета студента для преподавателя
    /// </summary>
    Task<GetTeacherReportDetailsResponse?> GetTeacherReportDetailsAsync(
        Guid reportId,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить файл версии отчета
    /// </summary>
    Task<LaboratoryReportFileDto?> GetReportFileAsync(
        Guid reportId,
        Guid versionId,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken);

    /// <summary>
    /// Проверить отчет студента
    /// </summary>
    Task<ReviewLaboratoryReportResponse> ReviewReportAsync(
        Guid teacherId,
        bool includeAll,
        Guid reportId,
        ReviewLaboratoryReportRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить ведомость студентов для преподавателя
    /// </summary>
    Task<PagedResultDto<TeacherGradebookItemDto>> GetTeacherGradebookAsync(
        GetTeacherGradebookRequest request,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken);

    /// <summary>
    /// Обновить запись ведомости студента
    /// </summary>
    Task<TeacherGradebookItemDto> UpdateGradebookAsync(
        Guid teacherId,
        Guid studentId,
        UpdateTeacherGradebookRequest request,
        bool includeAll,
        CancellationToken cancellationToken);
}
