namespace Application.DTO.Posts;

/// <summary>
/// Ответ на создание поста
/// </summary>
public sealed record CreatePostResponse
{
    public Guid Id { get; init; }
}
