using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер управления лабораторными работами преподавателем
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
[Route("api/v1/teacher/laboratories")]
public sealed class TeacherLaboratoriesController : PublicController
{
    private readonly ILaboratoryService _laboratoryService;

    public TeacherLaboratoriesController(ILaboratoryService laboratoryService)
    {
        _laboratoryService = laboratoryService;
    }

    /// <summary>
    /// Получить список лабораторных работ
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLaboratories(
        [FromQuery] GetTeacherLaboratoryListRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetTeacherLaboratoriesAsync(request, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Получить лабораторную работу
    /// </summary>
    [HttpGet("{laboratoryId:guid}")]
    public async Task<IActionResult> GetLaboratory(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetTeacherLaboratoryDetailsAsync(laboratoryId, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Создать лабораторную работу
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateLaboratory(
        CreateLaboratoryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.CreateLaboratoryAsync(request, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Обновить лабораторную работу
    /// </summary>
    [HttpPut("{laboratoryId:guid}")]
    public async Task<IActionResult> UpdateLaboratory(
        Guid laboratoryId,
        UpdateLaboratoryRequest request,
        CancellationToken cancellationToken)
    {
        await _laboratoryService.UpdateLaboratoryAsync(laboratoryId, request, cancellationToken);
        var result = Ok();

        return result;
    }

    /// <summary>
    /// Получить список уникальных блоков
    /// </summary>
    [HttpGet("blocks")]
    public async Task<IActionResult> GetBlocks(CancellationToken cancellationToken)
    {
        var blocks = await _laboratoryService.GetDistinctBlocksAsync(cancellationToken);
        return Ok(blocks);
    }

    /// <summary>
    /// Удалить лабораторную работу
    /// </summary>
    [HttpDelete("{laboratoryId:guid}")]
    public async Task<IActionResult> DeleteLaboratory(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        await _laboratoryService.DeleteLaboratoryAsync(laboratoryId, cancellationToken);
        var result = Ok();

        return result;
    }
}
