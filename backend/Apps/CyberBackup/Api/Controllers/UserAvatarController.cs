using Api.Controllers.Models.Request;
using Api.Controllers.Models.Response;
using Api.Filters;
using Api.Results;
using Api.Services.Avatars;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Avatars;
using Application.Abstractions.Services.Avatars.Contracts;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Аватары пользователей
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/user")]
public sealed class UserAvatarController : PublicController
{
    /// <summary>
    /// Лимит всего multipart-запроса загрузки; глобальный лимит Kestrel (1 МБ) не меняется
    /// </summary>
    private const long MaxUploadRequestBytes = 2 * 1024 * 1024;

    private readonly IJwtService _jwtService;
    private readonly IUserAvatarService _avatarService;
    private readonly IAvatarUploadRequestFactory _uploadRequestFactory;

    public UserAvatarController(
        IJwtService jwtService,
        IUserAvatarService avatarService,
        IAvatarUploadRequestFactory uploadRequestFactory)
    {
        _jwtService = jwtService;
        _avatarService = avatarService;
        _uploadRequestFactory = uploadRequestFactory;
    }

    /// <summary>
    /// Загрузить или заменить свой аватар
    /// </summary>
    [HttpPut("me/avatar")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxUploadRequestBytes)]
    [DisableFormValueProvider]
    public async Task<IActionResult> UploadMyAvatar([FromForm] UploadAvatarRequest request, CancellationToken token)
    {
        var currentUser = _jwtService.GetCurrentUser();
        var upload = _uploadRequestFactory.Create(request.File);
        var updatedAt = await _avatarService.SetAsync(currentUser.UserId, upload, token);

        return Ok(new AvatarUploadResponse(updatedAt));
    }

    /// <summary>
    /// Удалить свой аватар
    /// </summary>
    [HttpDelete("me/avatar")]
    public async Task<IActionResult> DeleteMyAvatar(CancellationToken token)
    {
        var currentUser = _jwtService.GetCurrentUser();

        await _avatarService.RemoveAsync(currentUser.UserId, token);

        return NoContent();
    }

    /// <summary>
    /// Получить аватар пользователя
    /// </summary>
    [HttpGet("{id:guid}/avatar")]
    public async Task<IActionResult> GetAvatar(Guid id, CancellationToken token)
    {
        var avatar = await _avatarService.GetAsync(id, Request.Headers.IfNoneMatch.ToString(), token);

        return new AvatarFileResult(avatar);
    }

    /// <summary>
    /// Удалить аватар пользователя (администратор)
    /// </summary>
    [HttpDelete("{id:guid}/avatar")]
    [Authorize(Roles = AuthRoleNames.AdminOrSuperAdmin)]
    public async Task<IActionResult> DeleteAvatar(Guid id, CancellationToken token)
    {
        await _avatarService.RemoveAsync(id, token);

        return NoContent();
    }
}
