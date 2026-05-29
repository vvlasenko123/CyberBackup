using Domain.Posts.Enums;

namespace Application.DTO.Posts;

/// <summary>
/// Элемент ленты новостей
/// </summary>
public sealed record PostItemDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string AuthorFullName { get; init; } = string.Empty;
    public PostCategory Category { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}
