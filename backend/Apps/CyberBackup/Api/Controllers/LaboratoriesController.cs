using Api.Auth;
using Application.Abstractions.Services.Laboratories;
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

    public LaboratoriesController(ILaboratoryService laboratoryService)
    {
        _laboratoryService = laboratoryService;
    }

    /// <summary>
    /// Получить список опубликованных лабораторных работ
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLaboratories(
        [FromQuery] GetLaboratoryListRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _laboratoryService.GetStudentLaboratoriesAsync(request, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Получить детали лабораторной работы
    /// </summary>
    [HttpGet("{laboratoryId:guid}")]
    public async Task<IActionResult> GetLaboratory(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(() => _laboratoryService.GetStudentLaboratoryDetailsAsync(laboratoryId, cancellationToken));
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
        return await ExecuteAsync(() => _laboratoryService.OpenHintAsync(laboratoryId, hintId, cancellationToken));
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
        return await ExecuteAsync(() => _laboratoryService.SubmitFlagAsync(laboratoryId, request, cancellationToken));
    }

    /// <summary>
    /// Загрузить отчет
    /// </summary>
    [HttpPost("{laboratoryId:guid}/reports")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadReport(
        Guid laboratoryId,
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return BadRequest(new
            {
                Code = "laboratory_report.file_required",
                Message = "Файл отчета обязателен"
            });
        }

        await using var stream = file.OpenReadStream();
        var request = new UploadLaboratoryReportFileDto(
            Content: stream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            Length: file.Length);

        return await ExecuteAsync(() => _laboratoryService.UploadReportAsync(laboratoryId, request, cancellationToken));
    }

    /// <summary>
    /// Получить свой отчет и историю версий
    /// </summary>
    [HttpGet("{laboratoryId:guid}/reports/my")]
    public async Task<IActionResult> GetMyReport(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(() => _laboratoryService.GetMyReportAsync(laboratoryId, cancellationToken));
    }

    /// <summary>
    /// Получить прогресс текущего студента
    /// </summary>
    [HttpGet("progress/my")]
    public async Task<IActionResult> GetMyProgress(CancellationToken cancellationToken)
    {
        var result = await _laboratoryService.GetMyProgressAsync(cancellationToken);

        return Ok(result);
    }

    private async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            return Ok(await action());
        }
        catch (LaboratoryException exception)
        {
            return BadRequest(new { exception.Code, exception.Message });
        }
    }
}
