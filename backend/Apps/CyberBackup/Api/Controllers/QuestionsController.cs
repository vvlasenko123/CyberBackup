using Api.Controllers.Models.Request.Questions;
using Api.Extensions;
using Application.Abstractions.Services.Questions.Contracts;
using Application.DTO.Questions;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер вопросов студента
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Student)]
[Route("api/v1/questions")]
public sealed class QuestionsController : PublicController
{
    private readonly IQuestionService _service;
    private readonly IQuestionAttachmentStorage _storage;

    public QuestionsController(IQuestionService service, IQuestionAttachmentStorage storage)
    {
        _service = service;
        _storage = storage;
    }

    /// <summary>Мои вопросы</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyQuestions(CancellationToken ct)
        => Ok(await _service.GetMyQuestionsAsync(ct));

    /// <summary>Детали вопроса</summary>
    [HttpGet("{questionId:guid}")]
    public async Task<IActionResult> GetMyQuestion(Guid questionId, CancellationToken ct)
        => Ok(await _service.GetMyQuestionDetailAsync(questionId, ct));

    /// <summary>Создать вопрос</summary>
    [HttpPost]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionRequest request, CancellationToken ct)
    {
        var id = await _service.CreateQuestionAsync(request, ct);
        return Ok(new { id });
    }

    /// <summary>Отправить сообщение в чат открытого вопроса</summary>
    [HttpPost("{questionId:guid}/message")]
    public async Task<IActionResult> SendMessage(
        Guid questionId,
        [FromBody] ReplyToQuestionRequest request,
        CancellationToken ct)
    {
        await _service.SendStudentMessageAsync(questionId, request, ct);
        return Ok();
    }

    /// <summary>Отправить сообщение с вложениями</summary>
    [HttpPost("{questionId:guid}/message/attachments")]
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
            await _service.SendStudentMessageAsync(questionId,
                new ReplyToQuestionRequest { Content = request.Content ?? string.Empty }, ct, files);
            return Ok();
        }
        finally
        {
            foreach (var file in files) await file.Content.DisposeAsync();
        }
    }

    [HttpGet("{questionId:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(
        Guid questionId,
        Guid attachmentId,
        CancellationToken ct,
        [FromQuery] bool inline = false)
    {
        var file = await _service.DownloadAttachmentAsync(questionId, attachmentId, ct);
        return this.ProtectedFile(file.ContentType, file.FileName, file.FileSizeBytes, inline,
            (destination, offset, length, token) => _storage.CopyToAsync(file.StoragePath, destination, offset, length, token));
    }

    /// <summary>Закрыть вопрос</summary>
    [HttpPost("{questionId:guid}/close")]
    public async Task<IActionResult> CloseQuestion(Guid questionId, CancellationToken ct)
    {
        await _service.CloseMyQuestionAsync(questionId, ct);
        return Ok();
    }
}
