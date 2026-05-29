using Application.Abstractions.Services.Posts.Contracts;
using Application.DTO.Posts;
using Domain.Posts.Enums;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер ленты новостей
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Student + "," + AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
[Route("api/v1/posts")]
public sealed class PostsController : PublicController
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    /// <summary>
    /// Получить список постов
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPosts(
        [FromQuery] PostCategory? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPostsRequest { Category = category, Page = page, PageSize = pageSize };
        var response = await _postService.GetPostsAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Создать пост (преподаватель / администратор)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
    public async Task<IActionResult> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _postService.CreatePostAsync(request, cancellationToken);
        return Ok(response);
    }
}
