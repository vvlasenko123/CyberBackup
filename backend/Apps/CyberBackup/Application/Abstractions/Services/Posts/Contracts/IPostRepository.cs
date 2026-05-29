using Application.DTO.Laboratories;
using Application.DTO.Posts;

namespace Application.Abstractions.Services.Posts.Contracts;

/// <summary>
/// Репозиторий постов (лента новостей)
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Получить список постов с пагинацией и фильтром по категории
    /// </summary>
    Task<PagedResultDto<PostItemDto>> GetPostsAsync(GetPostsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Создать пост
    /// </summary>
    Task<Guid> CreatePostAsync(Guid authorId, CreatePostRequest request, CancellationToken cancellationToken);
}
