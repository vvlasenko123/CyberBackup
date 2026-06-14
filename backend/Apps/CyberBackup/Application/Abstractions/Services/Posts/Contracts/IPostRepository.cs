using Application.DTO.Laboratories;
using Application.DTO.Posts;

namespace Application.Abstractions.Services.Posts.Contracts;

/// <summary>
/// Репозиторий постов (лента новостей)
/// </summary>
public interface IPostRepository
{
    /// <summary>
    /// Получить список постов с пагинацией и фильтром по категории.
    /// Для студента возвращаются только посты администраторов и преподавателей его групп.
    /// </summary>
    Task<PagedResultDto<PostItemDto>> GetPostsAsync(
        GetPostsRequest request,
        Guid currentUserId,
        bool filterByStudentTeachers,
        CancellationToken cancellationToken);

    /// <summary>
    /// Создать пост
    /// </summary>
    Task<Guid> CreatePostAsync(Guid authorId, CreatePostRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить пост
    /// </summary>
    Task DeletePostAsync(Guid postId, CancellationToken cancellationToken);
}
