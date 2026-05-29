using Application.Abstractions.Services.Questions.Contracts;
using Application.DTO.Laboratories;
using Application.DTO.Questions;
using Dapper;
using Domain.Questions.Enums;
using Domain.User.Enums;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class QuestionRepository : IQuestionRepository
{
    private readonly IAsyncDbConnection _connection;

    public QuestionRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyCollection<QuestionListItemDto>> GetMyQuestionsAsync(
        Guid studentId, CancellationToken ct)
    {
        var items = await _connection.QueryAsync<QuestionListItemDto>(
            """
            SELECT
                id AS "Id",
                laboratory_title AS "LaboratoryTitle",
                description AS "Description",
                status AS "Status",
                created_at_utc AS "CreatedAtUtc"
            FROM questions
            WHERE student_id = @StudentId
            ORDER BY created_at_utc DESC;
            """,
            new { StudentId = studentId },
            ct);

        return items.ToList();
    }

    public async Task<QuestionDetailDto?> GetQuestionDetailAsync(
        Guid questionId, Guid requesterId, CancellationToken ct)
    {
        await using var connection = await _connection.CreateConnectionAsync(ct);

        var question = await connection.QueryFirstOrDefaultAsync<QuestionDetailDbModel>(
            """
            SELECT
                q.id AS "Id",
                u.full_name AS "StudentFullName",
                g.name AS "StudentGroupName",
                q.laboratory_title AS "LaboratoryTitle",
                q.description AS "Description",
                q.status AS "Status",
                q.created_at_utc AS "CreatedAtUtc"
            FROM questions q
            JOIN users u ON u.id = q.student_id
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            LEFT JOIN groups g ON g.id = ug.group_id
            WHERE q.id = @QuestionId AND q.student_id = @RequesterId
            LIMIT 1;
            """,
            new { QuestionId = questionId, RequesterId = requesterId });

        if (question is null) return null;

        return await AttachMessagesAsync(connection, question);
    }

    public async Task<QuestionDetailDto?> GetQuestionDetailForTeacherAsync(
        Guid questionId, Guid teacherId, bool includeAll, CancellationToken ct)
    {
        await using var connection = await _connection.CreateConnectionAsync(ct);

        var question = await connection.QueryFirstOrDefaultAsync<QuestionDetailDbModel>(
            """
            SELECT
                q.id AS "Id",
                u.full_name AS "StudentFullName",
                g.name AS "StudentGroupName",
                q.laboratory_title AS "LaboratoryTitle",
                q.description AS "Description",
                q.status AS "Status",
                q.created_at_utc AS "CreatedAtUtc"
            FROM questions q
            JOIN users u ON u.id = q.student_id
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            LEFT JOIN groups g ON g.id = ug.group_id
            WHERE q.id = @QuestionId
              AND (
                  @IncludeAll = true
                  OR EXISTS (
                      SELECT 1 FROM teacher_groups tg
                      WHERE tg.teacher_id = @TeacherId AND tg.group_id = ug.group_id
                  )
              )
            LIMIT 1;
            """,
            new { QuestionId = questionId, TeacherId = teacherId, IncludeAll = includeAll });

        if (question is null) return null;

        return await AttachMessagesAsync(connection, question);
    }

    public async Task<Guid> CreateQuestionAsync(
        Guid studentId, CreateQuestionRequest request, CancellationToken ct)
    {
        var id = UUIDNext.Uuid.NewSequential();

        await _connection.ExecuteAsync(
            """
            INSERT INTO questions (id, student_id, laboratory_title, description, status, created_at_utc)
            VALUES (@Id, @StudentId, @LaboratoryTitle, @Description, @Status, @CreatedAtUtc);
            """,
            new
            {
                Id = id,
                StudentId = studentId,
                request.LaboratoryTitle,
                request.Description,
                Status = (int)QuestionStatus.Open,
                CreatedAtUtc = DateTimeOffset.UtcNow
            },
            ct);

        return id;
    }

    public async Task<bool> CloseQuestionAsync(Guid questionId, Guid studentId, CancellationToken ct)
    {
        var affected = await _connection.ExecuteAsync(
            """
            UPDATE questions
            SET status = @ClosedStatus
            WHERE id = @QuestionId AND student_id = @StudentId AND status != @ClosedStatus;
            """,
            new
            {
                QuestionId = questionId,
                StudentId = studentId,
                ClosedStatus = (int)QuestionStatus.Closed
            },
            ct);

        return affected > 0;
    }

    public async Task<PagedResultDto<TeacherQuestionListItemDto>> GetTeacherQuestionsAsync(
        GetTeacherQuestionsRequest request, Guid teacherId, bool includeAll, CancellationToken ct)
    {
        const string sql = """
                           SELECT COUNT(*)
                           FROM questions q
                           JOIN users u ON u.id = q.student_id
                           LEFT JOIN user_groups ug ON ug.user_id = u.id
                           LEFT JOIN groups g ON g.id = ug.group_id
                           WHERE (
                               @IncludeAll = true
                               OR EXISTS (
                                   SELECT 1 FROM teacher_groups tg
                                   WHERE tg.teacher_id = @TeacherId AND tg.group_id = ug.group_id
                               )
                           )
                           AND (@Status IS NULL OR q.status = @Status)
                           AND (@LaboratoryTitle IS NULL OR LOWER(q.laboratory_title) LIKE LOWER('%' || @LaboratoryTitle || '%'))
                           AND (@Search IS NULL OR LOWER(u.full_name) LIKE LOWER('%' || @Search || '%') OR g.name ILIKE @Search);

                           SELECT
                               q.id AS "Id",
                               u.full_name AS "StudentFullName",
                               g.name AS "GroupName",
                               q.laboratory_title AS "LaboratoryTitle",
                               q.description AS "Description",
                               q.status AS "Status",
                               q.created_at_utc AS "CreatedAtUtc"
                           FROM questions q
                           JOIN users u ON u.id = q.student_id
                           LEFT JOIN user_groups ug ON ug.user_id = u.id
                           LEFT JOIN groups g ON g.id = ug.group_id
                           WHERE (
                               @IncludeAll = true
                               OR EXISTS (
                                   SELECT 1 FROM teacher_groups tg
                                   WHERE tg.teacher_id = @TeacherId AND tg.group_id = ug.group_id
                               )
                           )
                           AND (@Status IS NULL OR q.status = @Status)
                           AND (@LaboratoryTitle IS NULL OR LOWER(q.laboratory_title) LIKE LOWER('%' || @LaboratoryTitle || '%'))
                           AND (@Search IS NULL OR LOWER(u.full_name) LIKE LOWER('%' || @Search || '%') OR g.name ILIKE @Search)
                           ORDER BY q.created_at_utc DESC
                           OFFSET @Offset LIMIT @PageSize;
                           """;

        await using var connection = await _connection.CreateConnectionAsync(ct);
        using var grid = await connection.QueryMultipleAsync(sql, new
        {
            TeacherId = teacherId,
            IncludeAll = includeAll,
            Status = (int?)request.Status,
            request.LaboratoryTitle,
            request.Search,
            Offset = (request.Page - 1) * request.PageSize,
            request.PageSize
        });

        var totalCount = await grid.ReadSingleAsync<int>();
        var items = (await grid.ReadAsync<TeacherQuestionListItemDto>()).ToList();

        return new PagedResultDto<TeacherQuestionListItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    public async Task<(bool Success, Guid StudentId)> ReplyToQuestionAsync(
        Guid questionId, Guid teacherId, bool includeAll, ReplyToQuestionRequest request, CancellationToken ct)
    {
        await using var connection = await _connection.CreateConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        var question = await connection.QueryFirstOrDefaultAsync<(Guid Id, Guid StudentId, int Status)>(
            """
            SELECT q.id AS "Id", q.student_id AS "StudentId", q.status AS "Status"
            FROM questions q
            JOIN users u ON u.id = q.student_id
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            WHERE q.id = @QuestionId
              AND q.status != @ClosedStatus
              AND (
                  @IncludeAll = true
                  OR EXISTS (
                      SELECT 1 FROM teacher_groups tg
                      WHERE tg.teacher_id = @TeacherId AND tg.group_id = ug.group_id
                  )
              )
            LIMIT 1
            FOR UPDATE OF q;
            """,
            new
            {
                QuestionId = questionId,
                TeacherId = teacherId,
                IncludeAll = includeAll,
                ClosedStatus = (int)QuestionStatus.Closed
            },
            transaction);

        if (question == default) return (false, Guid.Empty);

        // Вставляем новое сообщение (не upsert — разрешены несколько сообщений)
        await connection.ExecuteAsync(
            """
            INSERT INTO question_replies (id, question_id, sender_id, is_from_teacher, content, created_at_utc)
            VALUES (@Id, @QuestionId, @SenderId, true, @Content, @NowUtc);
            """,
            new
            {
                Id = UUIDNext.Uuid.NewSequential(),
                QuestionId = questionId,
                SenderId = teacherId,
                request.Content,
                NowUtc = DateTimeOffset.UtcNow
            },
            transaction);

        // Статус «Отвечен» ставим только при первом ответе преподавателя
        if (question.Status == (int)QuestionStatus.Open)
        {
            await connection.ExecuteAsync(
                "UPDATE questions SET status = @Status WHERE id = @QuestionId;",
                new { Status = (int)QuestionStatus.Answered, QuestionId = questionId },
                transaction);
        }

        await transaction.CommitAsync(ct);

        return (true, question.StudentId);
    }

    public async Task<bool> SendStudentMessageAsync(
        Guid questionId, Guid studentId, string content, CancellationToken ct)
    {
        await using var connection = await _connection.CreateConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        var lockedId = await connection.ExecuteScalarAsync<Guid?>(
            """
            SELECT id FROM questions
            WHERE id = @QuestionId AND student_id = @StudentId AND status != @ClosedStatus
            LIMIT 1
            FOR UPDATE;
            """,
            new { QuestionId = questionId, StudentId = studentId, ClosedStatus = (int)QuestionStatus.Closed },
            transaction);

        if (lockedId is null) return false;

        await connection.ExecuteAsync(
            """
            INSERT INTO question_replies (id, question_id, sender_id, is_from_teacher, content, created_at_utc)
            VALUES (@Id, @QuestionId, @SenderId, false, @Content, @NowUtc);
            """,
            new
            {
                Id = UUIDNext.Uuid.NewSequential(),
                QuestionId = questionId,
                SenderId = studentId,
                Content = content,
                NowUtc = DateTimeOffset.UtcNow
            },
            transaction);

        await transaction.CommitAsync(ct);
        return true;
    }

    public async Task<bool> CloseQuestionByTeacherAsync(
        Guid questionId, Guid teacherId, bool includeAll, CancellationToken ct)
    {
        var affected = await _connection.ExecuteAsync(
            """
            UPDATE questions q
            SET status = @ClosedStatus
            FROM users u
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            WHERE q.id = @QuestionId
              AND q.student_id = u.id
              AND q.status != @ClosedStatus
              AND (
                  @IncludeAll = true
                  OR EXISTS (
                      SELECT 1 FROM teacher_groups tg
                      WHERE tg.teacher_id = @TeacherId AND tg.group_id = ug.group_id
                  )
              );
            """,
            new { QuestionId = questionId, TeacherId = teacherId, IncludeAll = includeAll, ClosedStatus = (int)QuestionStatus.Closed },
            ct);

        return affected > 0;
    }

    public async Task<IReadOnlyCollection<string>> GetLaboratoryTitlesAsync(
        Guid teacherId, bool includeAll, CancellationToken ct)
    {
        var items = await _connection.QueryAsync<string>(
            """
            SELECT DISTINCT q.laboratory_title
            FROM questions q
            JOIN users u ON u.id = q.student_id
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            WHERE q.laboratory_title IS NOT NULL
              AND (
                  @IncludeAll = true
                  OR EXISTS (
                      SELECT 1 FROM teacher_groups tg
                      WHERE tg.teacher_id = @TeacherId AND tg.group_id = ug.group_id
                  )
              )
            ORDER BY q.laboratory_title;
            """,
            new { TeacherId = teacherId, IncludeAll = includeAll },
            ct);

        return items.ToList();
    }

    public async Task<IReadOnlyCollection<Guid>> GetTeacherIdsForStudentAsync(
        Guid studentId, CancellationToken ct)
    {
        var ids = await _connection.QueryAsync<Guid>(
            """
            SELECT DISTINCT tg.teacher_id
            FROM teacher_groups tg
            JOIN user_groups ug ON ug.group_id = tg.group_id
            WHERE ug.user_id = @StudentId;
            """,
            new { StudentId = studentId },
            ct);

        return ids.ToList();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static async Task<QuestionDetailDto> AttachMessagesAsync(
        Npgsql.NpgsqlConnection connection,
        QuestionDetailDbModel question)
    {
        var messages = await connection.QueryAsync<QuestionReplyDto>(
            """
            SELECT
                r.id AS "Id",
                u.full_name AS "SenderFullName",
                r.is_from_teacher AS "IsFromTeacher",
                r.content AS "Content",
                r.created_at_utc AS "CreatedAtUtc"
            FROM question_replies r
            JOIN users u ON u.id = r.sender_id
            WHERE r.question_id = @QuestionId
            ORDER BY r.created_at_utc ASC;
            """,
            new { QuestionId = question.Id });

        return new QuestionDetailDto
        {
            Id = question.Id,
            StudentFullName = question.StudentFullName,
            StudentGroupName = question.StudentGroupName,
            LaboratoryTitle = question.LaboratoryTitle,
            Description = question.Description,
            Status = question.Status,
            CreatedAtUtc = question.CreatedAtUtc,
            Messages = messages.ToList()
        };
    }

    private sealed record QuestionDetailDbModel
    {
        public Guid Id { get; init; }
        public string StudentFullName { get; init; } = string.Empty;
        public string? StudentGroupName { get; init; }
        public string? LaboratoryTitle { get; init; }
        public string Description { get; init; } = string.Empty;
        public QuestionStatus Status { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
    }
}
