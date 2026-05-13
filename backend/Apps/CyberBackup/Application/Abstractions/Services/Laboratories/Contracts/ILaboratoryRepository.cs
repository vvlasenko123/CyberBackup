using Application.DTO.Laboratories;

namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Репозиторий лабораторных работ
/// </summary>
public interface ILaboratoryRepository
{
    Task<PagedResultDto<GetLaboratoryListItemDto>> GetStudentLaboratoriesAsync(
        Guid studentId,
        GetLaboratoryListRequest request,
        CancellationToken cancellationToken);

    Task<GetLaboratoryDetailsResponse?> GetStudentLaboratoryDetailsAsync(
        Guid studentId,
        Guid laboratoryId,
        CancellationToken cancellationToken);

    Task<OpenLaboratoryHintResponse?> OpenHintAsync(
        Guid studentId,
        Guid laboratoryId,
        Guid hintId,
        CancellationToken cancellationToken);

    Task<SubmitLaboratoryFlagResponse> SubmitFlagAttemptAsync(
        Guid studentId,
        Guid laboratoryId,
        string submittedFlagHash,
        string submittedFlagMasked,
        bool isCorrect,
        CancellationToken cancellationToken);

    Task<UploadLaboratoryReportResponse> UploadReportAsync(
        Guid studentId,
        Guid laboratoryId,
        SavedLaboratoryReportFileDto file,
        CancellationToken cancellationToken);

    Task<GetMyLaboratoryReportResponse?> GetMyReportAsync(
        Guid studentId,
        Guid laboratoryId,
        CancellationToken cancellationToken);

    Task<GetMyProgressResponse> GetMyProgressAsync(
        Guid studentId,
        CancellationToken cancellationToken);

    Task<GetMyGradebookResponse?> GetMyGradebookAsync(
        Guid studentId,
        CancellationToken cancellationToken);

    Task<PagedResultDto<TeacherLaboratoryListItemDto>> GetTeacherLaboratoriesAsync(
        GetTeacherLaboratoryListRequest request,
        CancellationToken cancellationToken);

    Task<GetTeacherLaboratoryDetailsResponse?> GetTeacherLaboratoryDetailsAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    Task<string?> GetExpectedFlagHashAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    Task<Guid> CreateLaboratoryAsync(
        CreateLaboratoryRequest request,
        string? expectedFlagHash,
        CancellationToken cancellationToken);

    Task UpdateLaboratoryAsync(
        Guid laboratoryId,
        UpdateLaboratoryRequest request,
        string? expectedFlagHash,
        bool updateFlagHash,
        CancellationToken cancellationToken);

    Task DeleteLaboratoryAsync(Guid laboratoryId, CancellationToken cancellationToken);

    Task<PagedResultDto<TeacherReportListItemDto>> GetTeacherReportsAsync(
        GetTeacherReportListRequest request,
        CancellationToken cancellationToken);

    Task<GetTeacherReportDetailsResponse?> GetTeacherReportDetailsAsync(
        Guid reportId,
        CancellationToken cancellationToken);

    Task<LaboratoryReportFileDto?> GetReportFileAsync(
        Guid reportId,
        Guid versionId,
        CancellationToken cancellationToken);

    Task<ReviewLaboratoryReportResponse> ReviewReportAsync(
        Guid teacherId,
        Guid reportId,
        ReviewLaboratoryReportRequest request,
        CancellationToken cancellationToken);

    Task<PagedResultDto<TeacherGradebookItemDto>> GetTeacherGradebookAsync(
        GetTeacherGradebookRequest request,
        CancellationToken cancellationToken);

    Task<TeacherGradebookItemDto> UpdateGradebookAsync(
        Guid teacherId,
        Guid studentId,
        UpdateTeacherGradebookRequest request,
        CancellationToken cancellationToken);
}
