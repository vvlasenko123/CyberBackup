using Application.Abstractions.Services.Auth.Contracts;
using Application.DTO.Calendar;
using Domain.Repositories;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер уведомлений
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Student + "," + AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
[Route("api/v1/notifications")]
public sealed class NotificationsController : PublicController
{
    private readonly INotificationRepository _repository;
    private readonly IJwtService _jwtService;

    public NotificationsController(INotificationRepository repository, IJwtService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Получить последние уведомления текущего пользователя
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetNotifications(CancellationToken cancellationToken)
    {
        var user = _jwtService.GetCurrentUser();
        var models = await _repository.GetForUserAsync(user.UserId, limit: 50, cancellationToken);

        var result = models.Select(n => new GetNotificationDto(
            n.Id,
            n.Title,
            n.Message,
            n.IsRead,
            n.CreatedAtUtc)).ToList();

        return Ok(result);
    }

    /// <summary>
    /// Пометить все уведомления как прочитанные
    /// </summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var user = _jwtService.GetCurrentUser();
        await _repository.MarkAllReadAsync(user.UserId, cancellationToken);
        return Ok();
    }
}
