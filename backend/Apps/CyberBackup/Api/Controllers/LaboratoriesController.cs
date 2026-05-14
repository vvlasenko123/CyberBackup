using Api.Auth;
using Api.Controllers.Models.Request.Laboratories;
using Api.Services.Laboratories;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер лабораторных работ для студента
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.StudentTeacherAdmin)]
[Route("api/v1/laboratories")]
public sealed class LaboratoriesController : PublicController
{
    private readonly ILaboratoryService _laboratoryService;
    private readonly ILaboratoryReportUploadRequestFactory _uploadRequestFactory;

    public LaboratoriesController(
        ILaboratoryService laboratoryService,
        ILaboratoryReportUploadRequestFactory uploadRequestFactory)
    {
        _laboratoryService = laboratoryService;
        _uploadRequestFactory = uploadRequestFactory;
    }

    /// <summary>
    /// Получить список опубликованных лабораторных работ
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLaboratories(
        [FromQuery] GetLaboratoryListRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetStudentLaboratoriesAsync(request, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Получить детали лабораторной работы
    /// </summary>
    [HttpGet("{laboratoryId:guid}")]
    public async Task<IActionResult> GetLaboratory(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetStudentLaboratoryDetailsAsync(laboratoryId, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Открыть подсказку
    /// </summary>
    [HttpPost("{laboratoryId:guid}/hints/{hintId:guid}/open")]
    public async Task<IActionResult> OpenHint(
        Guid laboratoryId,
        Guid hintId,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.OpenHintAsync(laboratoryId, hintId, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Сдать флаг
    /// </summary>
    [HttpPost("{laboratoryId:guid}/flag")]
    public async Task<IActionResult> SubmitFlag(
        Guid laboratoryId,
        SubmitLaboratoryFlagRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.SubmitFlagAsync(laboratoryId, request, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Загрузить отчет
    /// </summary>
    [HttpPost("{laboratoryId:guid}/reports")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadReport(
        Guid laboratoryId,
        [FromForm] UploadLaboratoryReportRequest request,
        CancellationToken cancellationToken)
    {
        var file = _uploadRequestFactory.Create(request.File);
        var response = await _laboratoryService.UploadReportAsync(laboratoryId, file, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Получить свой отчет и историю версий
    /// </summary>
    [HttpGet("{laboratoryId:guid}/reports/my")]
    public async Task<IActionResult> GetMyReport(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetMyReportAsync(laboratoryId, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Получить прогресс текущего студента
    /// </summary>
    [HttpGet("progress/my")]
    public async Task<IActionResult> GetMyProgress(CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetMyProgressAsync(cancellationToken);
        var result = Ok(response);

        return result;
    }
}
