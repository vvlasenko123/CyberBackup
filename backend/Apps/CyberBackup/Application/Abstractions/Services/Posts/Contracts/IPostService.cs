using Application.DTO.Laboratories;
using Application.DTO.Posts;

namespace Application.Abstractions.Services.Posts.Contracts;

/// <summary>
/// Сервис постов (лента новостей)
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Получить список постов
    /// </summary>
    Task<PagedResultDto<PostItemDto>> GetPostsAsync(GetPostsRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Создать пост (только для учителя/администратора)
    /// </summary>
    Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request, CancellationToken cancellationToken);
}
