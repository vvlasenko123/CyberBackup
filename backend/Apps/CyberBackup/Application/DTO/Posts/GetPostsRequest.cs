using Domain.Posts.Enums;

namespace Application.DTO.Posts;

/// <summary>
/// Запрос получения постов
/// </summary>
public sealed record GetPostsRequest
{
    public PostCategory? Category { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
