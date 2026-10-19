using Api.Controllers.Models.Request.Questions;
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

    /// <summary>Отправить сообщение с вложениями</summary>
    [HttpPost("{questionId:guid}/reply/attachments")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(53 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 53 * 1024 * 1024)]
    public async Task<IActionResult> SendWithAttachments(Guid questionId,
        [FromForm] QuestionMessageUploadRequest request, CancellationToken ct)
    {
        if (request.Files.Count > 5 || request.Files.Any(f => f.Length == 0 || f.Length > 10 * 1024 * 1024))
            return BadRequest(new { message = "Допустимо до 5 непустых файлов по 10 МБ" });
        var files = new List<UploadQuestionAttachmentDto>();
        try
        {
            foreach (var file in request.Files)
                files.Add(new(file.OpenReadStream(), file.FileName, file.Length));
            await _service.ReplyToQuestionAsync(questionId,
                new ReplyToQuestionRequest { Content = request.Content ?? string.Empty }, ct, files);
            return Ok();
        }
        finally
        {
            foreach (var file in files) await file.Content.DisposeAsync();
        }
    }

    [HttpGet("{questionId:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(Guid questionId, Guid attachmentId, CancellationToken ct)
    {
        var file = await _service.DownloadAttachmentAsync(questionId, attachmentId, ct);
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        Response.Headers.CacheControl = "private, no-store";
        return File(file.Content, file.ContentType, file.FileName);
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
