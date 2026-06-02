using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер ведомости преподавателя
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
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
        var response = await _laboratoryService.GetTeacherGradebookAsync(request, cancellationToken);
        var result = Ok(response);

        return result;
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
        var response = await _laboratoryService.UpdateGradebookAsync(studentId, request, cancellationToken);
        var result = Ok(response);

        return result;
    }

    /// <summary>
    /// Экспортировать ведомость в Excel
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportGradebook(CancellationToken cancellationToken)
    {
        var bytes = await _laboratoryService.ExportGradebookAsync(cancellationToken);
        var fileName = $"Ведомость_{DateTime.UtcNow:yyyy-MM-dd}.xlsx";
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
