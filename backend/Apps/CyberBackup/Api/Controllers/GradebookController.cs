using Api.Auth;
using Application.Abstractions.Services.Laboratories.Contracts;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер ведомости студента
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoles.StudentTeacherAdmin)]
[Route("api/v1/gradebook")]
public sealed class GradebookController : PublicController
{
    private readonly ILaboratoryService _laboratoryService;

    public GradebookController(ILaboratoryService laboratoryService)
    {
        _laboratoryService = laboratoryService;
    }

    /// <summary>
    /// Получить ведомость текущего студента
    /// </summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyGradebook(CancellationToken cancellationToken)
    {
        var response = await _laboratoryService.GetMyGradebookAsync(cancellationToken);
        var result = Ok(response);

        return result;
    }
}
