using Api.Auth;
using Application.Abstractions.Services.Laboratories;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер ведомости преподавателя
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.TeacherAdmin)]
[Route("api/v1/teacher/gradebook")]
public sealed class TeacherGradebookController : PublicController
{
    private readonly ILaboratoryService _laboratoryService;

    public TeacherGradebookController(ILaboratoryService laboratoryService)
    {
        _laboratoryService = laboratoryService;
    }

    /// <summary>
    /// Получить ведомость студентов
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetGradebook(
        [FromQuery] GetTeacherGradebookRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _laboratoryService.GetTeacherGradebookAsync(request, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Обновить запись ведомости
    /// </summary>
    [HttpPut("{studentId:guid}")]
    public async Task<IActionResult> UpdateGradebook(
        Guid studentId,
        UpdateTeacherGradebookRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _laboratoryService.UpdateGradebookAsync(studentId, request, cancellationToken);

            return Ok(result);
        }
        catch (LaboratoryException exception)
        {
            return BadRequest(new { exception.Code, exception.Message });
        }
    }

    /// <summary>
    /// Экспорт ведомости
    /// </summary>
    [HttpGet("export")]
    public IActionResult ExportGradebook()
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            Code = "gradebook_export.not_supported",
            Message = "В проекте нет инфраструктуры экспорта Excel"
        });
    }
}
