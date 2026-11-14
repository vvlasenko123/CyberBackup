using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Posts.Contracts;
using Application.DTO.Laboratories;
using Application.DTO.Posts;
using Domain.User.Enums;
using Infrastructure.Exceptions;

namespace Application.Abstractions.Services.Posts;

/// <inheritdoc />
public sealed class PostService : IPostService
{
    private readonly IPostRepository _repository;
    private readonly IJwtService _jwtService;

    public PostService(IPostRepository repository, IJwtService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public Task<PagedResultDto<PostItemDto>> GetPostsAsync(GetPostsRequest request, CancellationToken cancellationToken)
    {
        var normalized = request with
        {
            Page = Math.Max(1, request.Page),
            PageSize = Math.Clamp(request.PageSize, 1, 50)
        };

        var currentUser = _jwtService.GetCurrentUser();
        var filterByStudentTeachers = currentUser.Role == UserRole.Student;

        return _repository.GetPostsAsync(normalized, currentUser.UserId, filterByStudentTeachers, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request, CancellationToken cancellationToken)
    {
        if (request.Title.Length > 255 || request.Content.Length > 2000 || !Enum.IsDefined(request.Category))
            throw new BusinessException(ErrorCodes.PostInvalid, "Проверьте категорию, заголовок до 255 символов и текст до 2000 символов");

        var currentUser = _jwtService.GetCurrentUser();
        var id = await _repository.CreatePostAsync(currentUser.UserId, request, cancellationToken);

        return new CreatePostResponse { Id = id };
    }

    /// <inheritdoc />
    public Task DeletePostAsync(Guid postId, CancellationToken cancellationToken)
    {
        return _repository.DeletePostAsync(postId, cancellationToken);
    }
}
