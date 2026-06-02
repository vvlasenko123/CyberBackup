using Application.Abstractions.Services.Posts.Contracts;
using Application.DTO.Laboratories;
using Application.DTO.Posts;
using Dapper;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class PostRepository : IPostRepository
{
    private readonly IAsyncDbConnection _connection;

    public PostRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<PostItemDto>> GetPostsAsync(
        GetPostsRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT COUNT(*)
                           FROM posts p
                           WHERE @Category IS NULL OR p.category = @Category;

                           SELECT
                               p.id AS "Id",
                               p.title AS "Title",
                               p.content AS "Content",
                               u.full_name AS "AuthorFullName",
                               p.category AS "Category",
                               p.created_at_utc AS "CreatedAtUtc"
                           FROM posts p
                           JOIN users u ON u.id = p.author_id
                           WHERE @Category IS NULL OR p.category = @Category
                           ORDER BY p.created_at_utc DESC
                           OFFSET @Offset LIMIT @PageSize;
                           """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(sql, new
        {
            Category = (int?)request.Category,
            Offset = (request.Page - 1) * request.PageSize,
            request.PageSize
        });

        var totalCount = await grid.ReadSingleAsync<int>();
        var items = (await grid.ReadAsync<PostItemDto>()).ToList();

        return new PagedResultDto<PostItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<Guid> CreatePostAsync(
        Guid authorId,
        CreatePostRequest request,
        CancellationToken cancellationToken)
    {
        var id = UUIDNext.Uuid.NewSequential();

        await _connection.ExecuteAsync(
            """
            INSERT INTO posts (id, author_id, title, content, category, created_at_utc)
            VALUES (@Id, @AuthorId, @Title, @Content, @Category, @CreatedAtUtc);
            """,
            new
            {
                Id = id,
                AuthorId = authorId,
                request.Title,
                request.Content,
                Category = (int)request.Category,
                CreatedAtUtc = DateTimeOffset.UtcNow
            },
            cancellationToken);

        return id;
    }

    /// <inheritdoc />
    public Task DeletePostAsync(Guid postId, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM posts WHERE id = @PostId";
        return _connection.ExecuteAsync(sql, new { PostId = postId }, cancellationToken);
    }
}
