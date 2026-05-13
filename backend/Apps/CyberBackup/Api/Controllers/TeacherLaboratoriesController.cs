using Api.Auth;
using Application.Abstractions.Services.Laboratories;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер управления лабораторными работами преподавателем
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.TeacherAdmin)]
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
        var result = await _laboratoryService.GetTeacherLaboratoriesAsync(request, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Получить лабораторную работу
    /// </summary>
    [HttpGet("{laboratoryId:guid}")]
    public async Task<IActionResult> GetLaboratory(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(() => _laboratoryService.GetTeacherLaboratoryDetailsAsync(laboratoryId, cancellationToken));
    }

    /// <summary>
    /// Создать лабораторную работу
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateLaboratory(
        CreateLaboratoryRequest request,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () => new { Id = await _laboratoryService.CreateLaboratoryAsync(request, cancellationToken) });
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
        return await ExecuteAsync(async () =>
        {
            await _laboratoryService.UpdateLaboratoryAsync(laboratoryId, request, cancellationToken);

            return new { Success = true };
        });
    }

    /// <summary>
    /// Удалить лабораторную работу
    /// </summary>
    [HttpDelete("{laboratoryId:guid}")]
    public async Task<IActionResult> DeleteLaboratory(
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        return await ExecuteAsync(async () =>
        {
            await _laboratoryService.DeleteLaboratoryAsync(laboratoryId, cancellationToken);

            return new { Success = true };
        });
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
