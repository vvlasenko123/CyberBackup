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

    public QuestionsController(IQuestionService service) => _service = service;

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

    /// <summary>Закрыть вопрос</summary>
    [HttpPost("{questionId:guid}/close")]
    public async Task<IActionResult> CloseQuestion(Guid questionId, CancellationToken ct)
    {
        await _service.CloseMyQuestionAsync(questionId, ct);
        return Ok();
    }
}
