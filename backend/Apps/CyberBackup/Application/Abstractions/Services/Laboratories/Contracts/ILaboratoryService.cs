using Application.DTO.Laboratories;

namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Сервис лабораторных работ
/// </summary>
public interface ILaboratoryService
{
    /// <summary>
    /// Получить список лабораторных работ для студента
    /// </summary>
    Task<PagedResultDto<GetLaboratoryListItemDto>> GetStudentLaboratoriesAsync(
        GetLaboratoryListRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали лабораторной работы для студента
    /// </summary>
    Task<GetLaboratoryDetailsResponse> GetStudentLaboratoryDetailsAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Открыть подсказку лабораторной работы
    /// </summary>
    Task<OpenLaboratoryHintResponse> OpenHintAsync(
        Guid laboratoryId,
        Guid hintId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Сдать флаг лабораторной работы
    /// </summary>
    Task<SubmitLaboratoryFlagResponse> SubmitFlagAsync(
        Guid laboratoryId,
        SubmitLaboratoryFlagRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Загрузить отчет по лабораторной работе
    /// </summary>
    Task<UploadLaboratoryReportResponse> UploadReportAsync(
        Guid laboratoryId,
        UploadLaboratoryReportFileDto file,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить отчет текущего студента по лабораторной работе
    /// </summary>
    Task<GetMyLaboratoryReportResponse> GetMyReportAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить прогресс текущего студента
    /// </summary>
    Task<GetMyProgressResponse> GetMyProgressAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить рейтинг группы текущего студента
    /// </summary>
    Task<GetGroupLeaderboardResponse> GetGroupLeaderboardAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить ведомость текущего студента
    /// </summary>
    Task<GetMyGradebookResponse> GetMyGradebookAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить список лабораторных работ для преподавателя
    /// </summary>
    Task<PagedResultDto<TeacherLaboratoryListItemDto>> GetTeacherLaboratoriesAsync(
        GetTeacherLaboratoryListRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали лабораторной работы для преподавателя
    /// </summary>
    Task<GetTeacherLaboratoryDetailsResponse> GetTeacherLaboratoryDetailsAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Создать лабораторную работу
    /// </summary>
    Task<CreateLaboratoryResponse> CreateLaboratoryAsync(
        CreateLaboratoryRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Обновить лабораторную работу
    /// </summary>
    Task UpdateLaboratoryAsync(
        Guid laboratoryId,
        UpdateLaboratoryRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Удалить лабораторную работу
    /// </summary>
    Task DeleteLaboratoryAsync(Guid laboratoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить список отчетов студентов для преподавателя
    /// </summary>
    Task<PagedResultDto<TeacherReportListItemDto>> GetTeacherReportsAsync(
        GetTeacherReportListRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить детали отчета студента для преподавателя
    /// </summary>
    Task<GetTeacherReportDetailsResponse> GetTeacherReportDetailsAsync(
        Guid reportId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить файл версии отчета (для студента — своего)
    /// </summary>
    Task<LaboratoryReportFileDto> GetStudentReportFileAsync(
        Guid laboratoryId,
        Guid versionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить файл версии отчета
    /// </summary>
    Task<LaboratoryReportFileDto> GetReportFileAsync(
        Guid reportId,
        Guid versionId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Проверить отчет студента
    /// </summary>
    Task<ReviewLaboratoryReportResponse> ReviewReportAsync(
        Guid reportId,
        ReviewLaboratoryReportRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Получить ведомость студентов для преподавателя
    /// </summary>
    Task<PagedResultDto<TeacherGradebookItemDto>> GetTeacherGradebookAsync(
        GetTeacherGradebookRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Обновить запись ведомости студента
    /// </summary>
    Task<TeacherGradebookItemDto> UpdateGradebookAsync(
        Guid studentId,
        UpdateTeacherGradebookRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Экспортировать ведомость в Excel
    /// </summary>
    Task<byte[]> ExportGradebookAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Получить список уникальных блоков лабораторных работ текущего преподавателя
    /// </summary>
    Task<IReadOnlyCollection<string>> GetDistinctBlocksAsync(CancellationToken cancellationToken);
}
