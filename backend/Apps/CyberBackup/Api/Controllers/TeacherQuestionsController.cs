using Application.Abstractions.Services.Questions.Contracts;
using Application.DTO.Questions;
using Domain.Questions.Enums;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер вопросов студентов для преподавателя
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
[Route("api/v1/teacher/questions")]
public sealed class TeacherQuestionsController : PublicController
{
    private readonly IQuestionService _service;

    public TeacherQuestionsController(IQuestionService service) => _service = service;

    /// <summary>Список вопросов студентов</summary>
    [HttpGet]
    public async Task<IActionResult> GetQuestions(
        [FromQuery] QuestionStatus? status,
        [FromQuery] string? laboratoryTitle,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var request = new GetTeacherQuestionsRequest
        {
            Status = status,
            LaboratoryTitle = laboratoryTitle,
            Search = search,
            Page = page,
            PageSize = pageSize
        };
        return Ok(await _service.GetTeacherQuestionsAsync(request, ct));
    }

    /// <summary>Детали вопроса</summary>
    [HttpGet("{questionId:guid}")]
    public async Task<IActionResult> GetQuestion(Guid questionId, CancellationToken ct)
        => Ok(await _service.GetTeacherQuestionDetailAsync(questionId, ct));

    /// <summary>Ответить на вопрос</summary>
    [HttpPost("{questionId:guid}/reply")]
    public async Task<IActionResult> Reply(
        Guid questionId,
        [FromBody] ReplyToQuestionRequest request,
        CancellationToken ct)
    {
        await _service.ReplyToQuestionAsync(questionId, request, ct);
        return Ok();
    }

    /// <summary>Закрыть вопрос</summary>
    [HttpPost("{questionId:guid}/close")]
    public async Task<IActionResult> CloseQuestion(Guid questionId, CancellationToken ct)
    {
        await _service.CloseQuestionByTeacherAsync(questionId, ct);
        return Ok();
    }

    /// <summary>Список уникальных лаб для фильтра</summary>
    [HttpGet("lab-titles")]
    public async Task<IActionResult> GetLabTitles(CancellationToken ct)
        => Ok(await _service.GetLaboratoryTitlesAsync(ct));
}
