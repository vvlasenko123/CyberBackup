using Domain.Posts.Enums;

namespace Application.DTO.Posts;

/// <summary>
/// Запрос создания поста
/// </summary>
public sealed record CreatePostRequest
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public PostCategory Category { get; init; }
}
