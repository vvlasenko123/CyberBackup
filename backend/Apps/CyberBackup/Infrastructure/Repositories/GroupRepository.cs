using Application.Abstractions.Services.Groups.Contracts;
using Application.DTO.Groups;
using Dapper;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class GroupRepository : IGroupRepository
{
    private readonly IAsyncDbConnection _connection;

    public GroupRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<GetGroupListItemDto>> GetGroupsAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT
                               g.id          AS "Id",
                               g.name        AS "Name",
                               g.created_at  AS "CreatedAt",
                               COUNT(DISTINCT ug.user_id)      AS "StudentCount",
                               COUNT(DISTINCT tg.teacher_id)   AS "TeacherCount"
                           FROM groups g
                           LEFT JOIN user_groups    ug ON ug.group_id = g.id
                           LEFT JOIN teacher_groups tg ON tg.group_id = g.id
                           GROUP BY g.id, g.name, g.created_at
                           ORDER BY g.name;
                           """;

        var result = await _connection.QueryAsync<GetGroupListItemDto>(sql, null, cancellationToken);
        return result.ToList();
    }

    /// <inheritdoc />
    public async Task<GetGroupDetailsResponse?> GetGroupDetailsAsync(Guid groupId, CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);

        var group = await connection.QueryFirstOrDefaultAsync<(Guid Id, string Name, DateTimeOffset CreatedAt)>(
            """
            SELECT id AS "Id", name AS "Name", created_at AS "CreatedAt"
            FROM groups
            WHERE id = @GroupId;
            """,
            new { GroupId = groupId });

        if (group == default)
            return null;

        var students = (await connection.QueryAsync<GroupMemberDto>(
            """
            SELECT
                u.id        AS "UserId",
                u.full_name AS "FullName",
                u.email     AS "Email"
            FROM user_groups ug
            JOIN users u ON u.id = ug.user_id
            WHERE ug.group_id = @GroupId
            ORDER BY u.full_name;
            """,
            new { GroupId = groupId })).ToList();

        var teachers = (await connection.QueryAsync<GroupMemberDto>(
            """
            SELECT
                u.id        AS "UserId",
                u.full_name AS "FullName",
                u.email     AS "Email"
            FROM teacher_groups tg
            JOIN users u ON u.id = tg.teacher_id
            WHERE tg.group_id = @GroupId
            ORDER BY u.full_name;
            """,
            new { GroupId = groupId })).ToList();

        return new GetGroupDetailsResponse
        {
            Id = group.Id,
            Name = group.Name,
            CreatedAt = group.CreatedAt,
            Students = students,
            Teachers = teachers
        };
    }

    /// <inheritdoc />
    public async Task<Guid> CreateGroupAsync(string name, CancellationToken cancellationToken)
    {
        var id = UUIDNext.Uuid.NewSequential();

        await _connection.ExecuteAsync(
            """
            INSERT INTO groups (id, name, created_at)
            VALUES (@Id, @Name, @CreatedAt);
            """,
            new { Id = id, Name = name, CreatedAt = DateTimeOffset.UtcNow },
            cancellationToken);

        return id;
    }

    /// <inheritdoc />
    public async Task DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        // user_groups не имеет FK на groups, удаляем вручную
        await connection.ExecuteAsync(
            "DELETE FROM user_groups WHERE group_id = @GroupId;",
            new { GroupId = groupId },
            transaction);

        // teacher_groups имеет ON DELETE CASCADE, но явно удаляем для надёжности
        await connection.ExecuteAsync(
            "DELETE FROM groups WHERE id = @GroupId;",
            new { GroupId = groupId },
            transaction);

        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddStudentToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        // Студент принадлежит ровно одной группе: убираем из текущей
        await connection.ExecuteAsync(
            "DELETE FROM user_groups WHERE user_id = @UserId;",
            new { UserId = userId },
            transaction);

        await connection.ExecuteAsync(
            """
            INSERT INTO user_groups (user_id, group_id)
            VALUES (@UserId, @GroupId)
            ON CONFLICT DO NOTHING;
            """,
            new { UserId = userId, GroupId = groupId },
            transaction);

        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveStudentFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        => _connection.ExecuteAsync(
            "DELETE FROM user_groups WHERE user_id = @UserId AND group_id = @GroupId;",
            new { UserId = userId, GroupId = groupId },
            cancellationToken);

    /// <inheritdoc />
    public Task AddTeacherToGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        => _connection.ExecuteAsync(
            """
            INSERT INTO teacher_groups (teacher_id, group_id, added_at_utc)
            VALUES (@UserId, @GroupId, @AddedAt)
            ON CONFLICT (teacher_id, group_id) DO NOTHING;
            """,
            new { UserId = userId, GroupId = groupId, AddedAt = DateTimeOffset.UtcNow },
            cancellationToken);

    /// <inheritdoc />
    public Task RemoveTeacherFromGroupAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
        => _connection.ExecuteAsync(
            "DELETE FROM teacher_groups WHERE teacher_id = @UserId AND group_id = @GroupId;",
            new { UserId = userId, GroupId = groupId },
            cancellationToken);
}
