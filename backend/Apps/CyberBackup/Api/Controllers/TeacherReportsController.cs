using Api.Auth;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер отчетов по лабораторным работам для преподавателя
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.TeacherAdmin)]
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
    /// Скачать файл версии отчета
    /// </summary>
    [HttpGet("{reportId:guid}/versions/{versionId:guid}/file")]
    public async Task<IActionResult> DownloadReportFile(
        Guid reportId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var file = await _laboratoryService.GetReportFileAsync(reportId, versionId, cancellationToken);
        var stream = await _fileStorage.OpenReadAsync(file.StoragePath, cancellationToken);
        var result = File(stream, file.ContentType, file.OriginalFileName);

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
