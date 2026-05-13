using Application.DTO.Laboratories;

namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Сервис лабораторных работ
/// </summary>
public interface ILaboratoryService
{
    Task<PagedResultDto<GetLaboratoryListItemDto>> GetStudentLaboratoriesAsync(
        GetLaboratoryListRequest request,
        CancellationToken cancellationToken);

    Task<GetLaboratoryDetailsResponse> GetStudentLaboratoryDetailsAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    Task<OpenLaboratoryHintResponse> OpenHintAsync(
        Guid laboratoryId,
        Guid hintId,
        CancellationToken cancellationToken);

    Task<SubmitLaboratoryFlagResponse> SubmitFlagAsync(
        Guid laboratoryId,
        SubmitLaboratoryFlagRequest request,
        CancellationToken cancellationToken);

    Task<UploadLaboratoryReportResponse> UploadReportAsync(
        Guid laboratoryId,
        UploadLaboratoryReportFileDto file,
        CancellationToken cancellationToken);

    Task<GetMyLaboratoryReportResponse> GetMyReportAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    Task<GetMyProgressResponse> GetMyProgressAsync(CancellationToken cancellationToken);

    Task<GetMyGradebookResponse> GetMyGradebookAsync(CancellationToken cancellationToken);

    Task<PagedResultDto<TeacherLaboratoryListItemDto>> GetTeacherLaboratoriesAsync(
        GetTeacherLaboratoryListRequest request,
        CancellationToken cancellationToken);

    Task<GetTeacherLaboratoryDetailsResponse> GetTeacherLaboratoryDetailsAsync(
        Guid laboratoryId,
        CancellationToken cancellationToken);

    Task<Guid> CreateLaboratoryAsync(
        CreateLaboratoryRequest request,
        CancellationToken cancellationToken);

    Task UpdateLaboratoryAsync(
        Guid laboratoryId,
        UpdateLaboratoryRequest request,
        CancellationToken cancellationToken);

    Task DeleteLaboratoryAsync(Guid laboratoryId, CancellationToken cancellationToken);

    Task<PagedResultDto<TeacherReportListItemDto>> GetTeacherReportsAsync(
        GetTeacherReportListRequest request,
        CancellationToken cancellationToken);

    Task<GetTeacherReportDetailsResponse> GetTeacherReportDetailsAsync(
        Guid reportId,
        CancellationToken cancellationToken);

    Task<LaboratoryReportFileDto> GetReportFileAsync(
        Guid reportId,
        Guid versionId,
        CancellationToken cancellationToken);

    Task<ReviewLaboratoryReportResponse> ReviewReportAsync(
        Guid reportId,
        ReviewLaboratoryReportRequest request,
        CancellationToken cancellationToken);

    Task<PagedResultDto<TeacherGradebookItemDto>> GetTeacherGradebookAsync(
        GetTeacherGradebookRequest request,
        CancellationToken cancellationToken);

    Task<TeacherGradebookItemDto> UpdateGradebookAsync(
        Guid studentId,
        UpdateTeacherGradebookRequest request,
        CancellationToken cancellationToken);
}
