using Api.Extensions;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер отчетов по лабораторным работам для преподавателя
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
[Route("api/v1/teacher/reports")]
public sealed class TeacherReportsController : PublicController
{
    private readonly ILaboratoryService _laboratoryService;
    private readonly ILaboratoryReportFileStorage _fileStorage;

    public TeacherReportsController(
        ILaboratoryService laboratoryService,
        ILaboratoryReportFileStorage fileStorage)
    {
        _laboratoryService = laboratoryService;
        _fileStorage = fileStorage;
    }

    /// <summary>
    /// Получить список отчетов студентов
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] GetTeacherReportListRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetTeacherReportsAsync(request, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Получить детали отчета
    /// </summary>
    [HttpGet("{reportId:guid}")]
    public async Task<IActionResult> GetReport(
        Guid reportId,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetTeacherReportDetailsAsync(reportId, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Скачать файл версии отчета или открыть его в браузере
    /// </summary>
    [HttpGet("{reportId:guid}/versions/{versionId:guid}/file")]
    public async Task<IActionResult> DownloadReportFile(
        Guid reportId,
        Guid versionId,
        CancellationToken cancellationToken,
        [FromQuery] bool inline = false)
    {
        var file = await _laboratoryService.GetReportFileAsync(reportId, versionId, cancellationToken);
        var result = this.ProtectedFile(file.ContentType, file.OriginalFileName, file.FileSizeBytes, inline,
            (destination, offset, length, ct) => _fileStorage.CopyToAsync(file.StoragePath, destination, offset, length, ct));

        return result;
    }

    /// <summary>
    /// Проверить последнюю версию отчета
    /// </summary>
    [HttpPost("{reportId:guid}/review")]
    public async Task<IActionResult> ReviewReport(
        Guid reportId,
        ReviewLaboratoryReportRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.ReviewReportAsync(reportId, request, cancellationToken);
        var result = Ok(response);

        return result;
    }
}
