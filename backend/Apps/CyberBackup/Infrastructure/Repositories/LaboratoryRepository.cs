using Application.Abstractions.Services.Laboratories;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Dapper;
using Domain.Laboratories.Enums;
using Domain.User.Enums;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class LaboratoryRepository : ILaboratoryRepository
{
    private readonly IAsyncDbConnection _connection;

    public LaboratoryRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<GetLaboratoryListItemDto>> GetStudentLaboratoriesAsync(
        Guid studentId,
        GetLaboratoryListRequest request,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           WITH penalties AS (
                               SELECT sh.laboratory_work_id, COALESCE(SUM(h.penalty_points), 0)::int AS penalty_points
                               FROM student_laboratory_hints sh
                               JOIN laboratory_hints h ON h.id = sh.laboratory_hint_id
                               WHERE sh.student_id = @StudentId
                               GROUP BY sh.laboratory_work_id
                           ),
                           flags AS (
                               SELECT fa.laboratory_work_id, BOOL_OR(fa.is_correct) AS is_correct
                               FROM laboratory_flag_attempts fa
                               WHERE fa.student_id = @StudentId
                               GROUP BY fa.laboratory_work_id
                           ),
                           source AS (
                               SELECT
                                   lw.id AS "Id",
                                   lw.title AS "Title",
                                   lw.short_description AS "ShortDescription",
                                   lw.difficulty AS "Difficulty",
                                   lw.block AS "Block",
                                   lw.max_points AS "MaxPoints",
                                   GREATEST(COALESCE(r.points, CASE WHEN COALESCE(f.is_correct, false) THEN lw.max_points - COALESCE(p.penalty_points, 0) ELSE 0 END), 0)::int AS "EarnedPoints",
                                   CASE
                                       WHEN r.status = 4 OR COALESCE(f.is_correct, false) THEN 3
                                       WHEN r.status IN (1, 2) THEN 2
                                       WHEN r.status = 3 THEN 4
                                       WHEN lp.id IS NOT NULL OR r.id IS NOT NULL OR COALESCE(p.penalty_points, 0) > 0 THEN 1
                                       ELSE 0
                                   END AS "Status",
                                   CASE
                                       WHEN r.status = 4 OR COALESCE(f.is_correct, false) THEN true
                                       ELSE false
                                   END AS "IsCompleted",
                                   CASE
                                       WHEN r.status = 4 OR COALESCE(f.is_correct, false) THEN 100
                                       WHEN r.status IN (1, 2) THEN 50
                                       WHEN r.status = 3 THEN 25
                                       WHEN lp.id IS NOT NULL OR r.id IS NOT NULL OR COALESCE(p.penalty_points, 0) > 0 THEN 10
                                       ELSE 0
                                   END AS "ProgressPercent",
                                   lw.sort_order AS "SortOrder"
                               FROM laboratory_works lw
                               LEFT JOIN laboratory_reports r ON r.laboratory_work_id = lw.id AND r.student_id = @StudentId
                               LEFT JOIN laboratory_progress lp ON lp.laboratory_work_id = lw.id AND lp.student_id = @StudentId
                               LEFT JOIN penalties p ON p.laboratory_work_id = lw.id
                               LEFT JOIN flags f ON f.laboratory_work_id = lw.id
                               WHERE lw.is_published = true
                                 AND lw.delete_date_utc IS NULL
                                 AND (@Block IS NULL OR lw.block = @Block)
                                 AND (@Difficulty IS NULL OR lw.difficulty = @Difficulty)
                                 AND (@Search IS NULL OR LOWER(lw.title) LIKE LOWER('%' || @Search || '%'))
                           )
                           SELECT COUNT(*) FROM source WHERE (@Status IS NULL OR "Status" = @Status);

                           WITH penalties AS (
                               SELECT sh.laboratory_work_id, COALESCE(SUM(h.penalty_points), 0)::int AS penalty_points
                               FROM student_laboratory_hints sh
                               JOIN laboratory_hints h ON h.id = sh.laboratory_hint_id
                               WHERE sh.student_id = @StudentId
                               GROUP BY sh.laboratory_work_id
                           ),
                           flags AS (
                               SELECT fa.laboratory_work_id, BOOL_OR(fa.is_correct) AS is_correct
                               FROM laboratory_flag_attempts fa
                               WHERE fa.student_id = @StudentId
                               GROUP BY fa.laboratory_work_id
                           ),
                           source AS (
                               SELECT
                                   lw.id AS "Id",
                                   lw.title AS "Title",
                                   lw.short_description AS "ShortDescription",
                                   lw.difficulty AS "Difficulty",
                                   lw.block AS "Block",
                                   lw.max_points AS "MaxPoints",
                                   GREATEST(COALESCE(r.points, CASE WHEN COALESCE(f.is_correct, false) THEN lw.max_points - COALESCE(p.penalty_points, 0) ELSE 0 END), 0)::int AS "EarnedPoints",
                                   CASE
                                       WHEN r.status = 4 OR COALESCE(f.is_correct, false) THEN 3
                                       WHEN r.status IN (1, 2) THEN 2
                                       WHEN r.status = 3 THEN 4
                                       WHEN lp.id IS NOT NULL OR r.id IS NOT NULL OR COALESCE(p.penalty_points, 0) > 0 THEN 1
                                       ELSE 0
                                   END AS "Status",
                                   CASE
                                       WHEN r.status = 4 OR COALESCE(f.is_correct, false) THEN true
                                       ELSE false
                                   END AS "IsCompleted",
                                   CASE
                                       WHEN r.status = 4 OR COALESCE(f.is_correct, false) THEN 100
                                       WHEN r.status IN (1, 2) THEN 50
                                       WHEN r.status = 3 THEN 25
                                       WHEN lp.id IS NOT NULL OR r.id IS NOT NULL OR COALESCE(p.penalty_points, 0) > 0 THEN 10
                                       ELSE 0
                                   END AS "ProgressPercent",
                                   lw.sort_order AS "SortOrder"
                               FROM laboratory_works lw
                               LEFT JOIN laboratory_reports r ON r.laboratory_work_id = lw.id AND r.student_id = @StudentId
                               LEFT JOIN laboratory_progress lp ON lp.laboratory_work_id = lw.id AND lp.student_id = @StudentId
                               LEFT JOIN penalties p ON p.laboratory_work_id = lw.id
                               LEFT JOIN flags f ON f.laboratory_work_id = lw.id
                               WHERE lw.is_published = true
                                 AND lw.delete_date_utc IS NULL
                                 AND (@Block IS NULL OR lw.block = @Block)
                                 AND (@Difficulty IS NULL OR lw.difficulty = @Difficulty)
                                 AND (@Search IS NULL OR LOWER(lw.title) LIKE LOWER('%' || @Search || '%'))
                           )
                           SELECT * FROM source
                           WHERE (@Status IS NULL OR "Status" = @Status)
                           ORDER BY "SortOrder", "Title"
                           OFFSET @Offset LIMIT @PageSize;
                           """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(sql, new
        {
            StudentId = studentId,
            request.Block,
            Difficulty = (int?)request.Difficulty,
            Status = (int?)request.Status,
            request.Search,
            Offset = (request.Page - 1) * request.PageSize,
            request.PageSize
        });

        var totalCount = await grid.ReadSingleAsync<int>();
        var items = (await grid.ReadAsync<GetLaboratoryListItemDto>())
            .Select(x => x with
            {
                DifficultyName = GetDifficultyName(x.Difficulty),
                StatusName = GetStudentStatusName(x.Status)
            })
            .ToList();

        return new PagedResultDto<GetLaboratoryListItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<GetLaboratoryDetailsResponse?> GetStudentLaboratoryDetailsAsync(
        Guid studentId,
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        const string laboratorySql = """
                                     WITH penalties AS (
                                         SELECT COALESCE(SUM(h.penalty_points), 0)::int AS penalty_points
                                         FROM student_laboratory_hints sh
                                         JOIN laboratory_hints h ON h.id = sh.laboratory_hint_id
                                         WHERE sh.student_id = @StudentId AND sh.laboratory_work_id = @LaboratoryId
                                     ),
                                     flags AS (
                                         SELECT BOOL_OR(is_correct) AS is_correct
                                         FROM laboratory_flag_attempts
                                         WHERE student_id = @StudentId AND laboratory_work_id = @LaboratoryId
                                     )
                                     SELECT
                                         lw.id AS "Id",
                                         lw.title AS "Title",
                                         lw.short_description AS "ShortDescription",
                                         lw.description AS "Description",
                                         lw.narrative AS "Narrative",
                                         lw.goal AS "Goal",
                                         lw.environment_url AS "EnvironmentUrl",
                                         lw.credentials AS "Credentials",
                                         lw.difficulty AS "Difficulty",
                                         lw.block AS "Block",
                                         lw.max_points AS "MaxPoints",
                                         GREATEST(COALESCE(r.points, CASE WHEN COALESCE(f.is_correct, false) THEN lw.max_points - COALESCE(p.penalty_points, 0) ELSE 0 END), 0)::int AS "EarnedPoints",
                                         lw.has_flag AS "HasFlag",
                                         COALESCE(f.is_correct, false) AS "FlagAlreadySubmitted",
                                         COALESCE(r.status, 0) AS "ReportStatus",
                                         CASE
                                             WHEN r.id IS NULL THEN true
                                             WHEN r.status IN (1, 2, 4) THEN false
                                             WHEN r.allow_resubmit = true THEN true
                                             ELSE false
                                         END AS "AllowReportUpload",
                                         COALESCE(r.allow_resubmit, false) AS "CanResubmitReport"
                                     FROM laboratory_works lw
                                     LEFT JOIN laboratory_reports r ON r.laboratory_work_id = lw.id AND r.student_id = @StudentId
                                     LEFT JOIN penalties p ON true
                                     LEFT JOIN flags f ON true
                                     WHERE lw.id = @LaboratoryId
                                       AND lw.is_published = true
                                       AND lw.delete_date_utc IS NULL
                                     LIMIT 1;
                                     """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        var laboratory = await connection.QueryFirstOrDefaultAsync<GetLaboratoryDetailsResponse>(
            laboratorySql,
            new { StudentId = studentId, LaboratoryId = laboratoryId });

        if (laboratory is null)
        {
            return null;
        }

        await EnsureProgressStartedAsync(connection, studentId, laboratoryId);

        var hints = await connection.QueryAsync<LaboratoryHintDto>(
            """
            SELECT
                h.id AS "Id",
                h.order_number AS "OrderNumber",
                h.title AS "Title",
                h.penalty_points AS "PenaltyPoints",
                CASE WHEN sh.id IS NULL THEN false ELSE true END AS "IsOpened",
                CASE WHEN sh.id IS NULL THEN NULL ELSE h.text END AS "Text"
            FROM laboratory_hints h
            LEFT JOIN student_laboratory_hints sh ON sh.laboratory_hint_id = h.id AND sh.student_id = @StudentId
            WHERE h.laboratory_work_id = @LaboratoryId
            ORDER BY h.order_number;
            """,
            new { StudentId = studentId, LaboratoryId = laboratoryId });

        var report = await GetMyReportInternalAsync(connection, studentId, laboratoryId);

        return laboratory with
        {
            DifficultyName = GetDifficultyName(laboratory.Difficulty),
            Hints = hints.ToList(),
            Report = report
        };
    }

    /// <inheritdoc />
    public async Task<OpenLaboratoryHintResponse?> OpenHintAsync(
        Guid studentId,
        Guid laboratoryId,
        Guid hintId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var hint = await connection.QueryFirstOrDefaultAsync<LaboratoryHintDto>(
            """
            SELECT
                h.id AS "Id",
                h.order_number AS "OrderNumber",
                h.title AS "Title",
                h.penalty_points AS "PenaltyPoints",
                false AS "IsOpened",
                h.text AS "Text"
            FROM laboratory_hints h
            JOIN laboratory_works lw ON lw.id = h.laboratory_work_id
            WHERE h.id = @HintId
              AND h.laboratory_work_id = @LaboratoryId
              AND lw.is_published = true
              AND lw.delete_date_utc IS NULL
            LIMIT 1;
            """,
            new { HintId = hintId, LaboratoryId = laboratoryId },
            transaction);

        if (hint is null)
        {
            return null;
        }

        await EnsureProgressStartedAsync(connection, studentId, laboratoryId, transaction);

        await connection.ExecuteAsync(
            """
            INSERT INTO student_laboratory_hints (id, student_id, laboratory_work_id, laboratory_hint_id, open_date_utc)
            VALUES (@Id, @StudentId, @LaboratoryId, @HintId, @OpenDateUtc)
            ON CONFLICT (student_id, laboratory_hint_id) DO NOTHING;
            """,
            new
            {
                Id = UUIDNext.Uuid.NewSequential(),
                StudentId = studentId,
                LaboratoryId = laboratoryId,
                HintId = hintId,
                OpenDateUtc = DateTimeOffset.UtcNow
            },
            transaction);

        var penalty = await connection.QuerySingleAsync<(int TotalPenaltyPoints, int AvailablePoints)>(
            """
            SELECT
                COALESCE(SUM(h.penalty_points), 0)::int AS "TotalPenaltyPoints",
                GREATEST(lw.max_points - COALESCE(SUM(h.penalty_points), 0), 0)::int AS "AvailablePoints"
            FROM laboratory_works lw
            LEFT JOIN student_laboratory_hints sh ON sh.laboratory_work_id = lw.id AND sh.student_id = @StudentId
            LEFT JOIN laboratory_hints h ON h.id = sh.laboratory_hint_id
            WHERE lw.id = @LaboratoryId
            GROUP BY lw.max_points;
            """,
            new { StudentId = studentId, LaboratoryId = laboratoryId },
            transaction);

        await transaction.CommitAsync(cancellationToken);

        return new OpenLaboratoryHintResponse
        {
            HintId = hint.Id,
            OrderNumber = hint.OrderNumber,
            Text = hint.Text ?? string.Empty,
            PenaltyPoints = hint.PenaltyPoints,
            TotalPenaltyPoints = penalty.TotalPenaltyPoints,
            AvailablePoints = penalty.AvailablePoints
        };
    }

    /// <inheritdoc />
    public async Task<SubmitLaboratoryFlagResponse> SubmitFlagAttemptAsync(
        Guid studentId,
        Guid laboratoryId,
        string submittedFlagHash,
        string submittedFlagMasked,
        bool isCorrect,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           INSERT INTO laboratory_progress (
                               id, laboratory_work_id, student_id, status, started_at_utc, completed_at_utc
                           )
                           VALUES (
                               @ProgressId, @LaboratoryId, @StudentId, @InProgressStatus, @CreateDateUtc, NULL
                           )
                           ON CONFLICT (laboratory_work_id, student_id) DO NOTHING;

                           INSERT INTO laboratory_flag_attempts (
                               id, student_id, laboratory_work_id, submitted_flag_hash,
                               submitted_flag_masked, is_correct, create_date_utc
                           )
                           VALUES (
                               @Id, @StudentId, @LaboratoryId, @SubmittedFlagHash,
                               @SubmittedFlagMasked, @IsCorrect, @CreateDateUtc
                           );

                           UPDATE laboratory_progress
                           SET status = @CompletedStatus,
                               completed_at_utc = @CreateDateUtc
                           WHERE laboratory_work_id = @LaboratoryId
                             AND student_id = @StudentId
                             AND @IsCorrect = true;

                           SELECT
                               GREATEST(lw.max_points - COALESCE(SUM(h.penalty_points), 0), 0)::int
                           FROM laboratory_works lw
                           LEFT JOIN student_laboratory_hints sh ON sh.laboratory_work_id = lw.id AND sh.student_id = @StudentId
                           LEFT JOIN laboratory_hints h ON h.id = sh.laboratory_hint_id
                           WHERE lw.id = @LaboratoryId
                           GROUP BY lw.max_points;
                           """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(sql, new
        {
            ProgressId = UUIDNext.Uuid.NewSequential(),
            InProgressStatus = (int)StudentLaboratoryStatus.InProgress,
            CompletedStatus = (int)StudentLaboratoryStatus.Accepted,
            Id = UUIDNext.Uuid.NewSequential(),
            StudentId = studentId,
            LaboratoryId = laboratoryId,
            SubmittedFlagHash = submittedFlagHash,
            SubmittedFlagMasked = submittedFlagMasked,
            IsCorrect = isCorrect,
            CreateDateUtc = DateTimeOffset.UtcNow
        });

        var earnedPoints = await grid.ReadSingleAsync<int>();

        return new SubmitLaboratoryFlagResponse
        {
            IsCorrect = isCorrect,
            Message = isCorrect ? "Флаг принят" : "Флаг неверный",
            EarnedPoints = isCorrect ? earnedPoints : 0,
            Status = isCorrect ? nameof(LaboratoryReportStatus.Accepted) : nameof(StudentLaboratoryStatus.InProgress)
        };
    }

    /// <inheritdoc />
    public async Task<UploadLaboratoryReportResponse> UploadReportAsync(
        Guid studentId,
        Guid laboratoryId,
        SavedLaboratoryReportFileDto file,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var laboratoryExists = await connection.QuerySingleAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1 FROM laboratory_works
                WHERE id = @LaboratoryId AND is_published = true AND delete_date_utc IS NULL
            );
            """,
            new { LaboratoryId = laboratoryId },
            transaction);

        if (!laboratoryExists)
        {
            throw new LaboratoryException("laboratory.not_found", "Лабораторная работа не найдена");
        }

        await EnsureProgressStartedAsync(connection, studentId, laboratoryId, transaction);

        var report = await connection.QueryFirstOrDefaultAsync<ReportStateDbModel>(
            """
            SELECT
                id AS "Id",
                status AS "Status",
                current_version_number AS "CurrentVersionNumber",
                allow_resubmit AS "AllowResubmit"
            FROM laboratory_reports
            WHERE student_id = @StudentId AND laboratory_work_id = @LaboratoryId
            FOR UPDATE;
            """,
            new { StudentId = studentId, LaboratoryId = laboratoryId },
            transaction);

        if (report?.Status is LaboratoryReportStatus.Submitted or LaboratoryReportStatus.UnderReview)
        {
            throw new LaboratoryException("laboratory_report.pending_review", "Предыдущая версия отчета ожидает проверки");
        }

        if (report?.Status == LaboratoryReportStatus.Accepted)
        {
            throw new LaboratoryException("laboratory_report.accepted_final", "Принятый отчет нельзя отправить повторно");
        }

        if (report is not null && !report.AllowResubmit)
        {
            throw new LaboratoryException("laboratory_report.resubmit_forbidden", "Повторная отправка отчета запрещена");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var reportId = report?.Id ?? UUIDNext.Uuid.NewSequential();
        var versionId = UUIDNext.Uuid.NewSequential();
        var versionNumber = (report?.CurrentVersionNumber ?? 0) + 1;

        if (report is null)
        {
            await connection.ExecuteAsync(
                """
                INSERT INTO laboratory_reports (
                    id, student_id, laboratory_work_id, status, current_version_number,
                    points, teacher_comment, allow_resubmit, create_date_utc, update_date_utc
                )
                VALUES (
                    @ReportId, @StudentId, @LaboratoryId, @Status, @VersionNumber,
                    NULL, NULL, false, @NowUtc, NULL
                );
                """,
                new
                {
                    ReportId = reportId,
                    StudentId = studentId,
                    LaboratoryId = laboratoryId,
                    Status = (int)LaboratoryReportStatus.Submitted,
                    VersionNumber = versionNumber,
                    NowUtc = nowUtc
                },
                transaction);
        }
        else
        {
            await connection.ExecuteAsync(
                """
                UPDATE laboratory_reports
                SET status = @Status,
                    current_version_number = @VersionNumber,
                    points = NULL,
                    teacher_comment = NULL,
                    allow_resubmit = false,
                    update_date_utc = @NowUtc
                WHERE id = @ReportId;
                """,
                new
                {
                    ReportId = reportId,
                    Status = (int)LaboratoryReportStatus.Submitted,
                    VersionNumber = versionNumber,
                    NowUtc = nowUtc
                },
                transaction);
        }

        await connection.ExecuteAsync(
            """
            INSERT INTO laboratory_report_versions (
                id, laboratory_report_id, version_number, storage_path, original_file_name,
                content_type, file_size_bytes, status, points, teacher_comment,
                checked_by_teacher_id, checked_date_utc, allow_resubmit_after_review,
                create_date_utc
            )
            VALUES (
                @VersionId, @ReportId, @VersionNumber, @StoragePath, @OriginalFileName,
                @ContentType, @FileSizeBytes, @Status, NULL, NULL,
                NULL, NULL, NULL, @NowUtc
            );
            """,
            new
            {
                VersionId = versionId,
                ReportId = reportId,
                VersionNumber = versionNumber,
                file.StoragePath,
                file.OriginalFileName,
                file.ContentType,
                file.FileSizeBytes,
                Status = (int)LaboratoryReportStatus.Submitted,
                NowUtc = nowUtc
            },
            transaction);

        await transaction.CommitAsync(cancellationToken);

        return new UploadLaboratoryReportResponse
        {
            ReportId = reportId,
            VersionId = versionId,
            VersionNumber = versionNumber,
            Status = LaboratoryReportStatus.Submitted,
            FileName = file.OriginalFileName,
            FileSizeBytes = file.FileSizeBytes,
            CreateDateUtc = nowUtc
        };
    }

    /// <inheritdoc />
    public async Task<GetMyLaboratoryReportResponse?> GetMyReportAsync(
        Guid studentId,
        Guid laboratoryId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);

        return await GetMyReportInternalAsync(connection, studentId, laboratoryId);
    }

    /// <inheritdoc />
    public async Task<GetMyProgressResponse> GetMyProgressAsync(Guid studentId, CancellationToken cancellationToken)
    {
        var laboratories = await GetStudentLaboratoriesAsync(
            studentId,
            new GetLaboratoryListRequest { Page = 1, PageSize = 100 },
            cancellationToken);

        var items = laboratories.Items
            .Select(x => new MyProgressLaboratoryDto
            {
                LaboratoryId = x.Id,
                Title = x.Title,
                Status = x.Status,
                EarnedPoints = x.EarnedPoints,
                MaxPoints = x.MaxPoints
            })
            .ToList();

        var completed = items.Count(x => x.Status == StudentLaboratoryStatus.Accepted);
        var pending = items.Count(x => x.Status == StudentLaboratoryStatus.PendingReview);
        var rejected = items.Count(x => x.Status == StudentLaboratoryStatus.RevisionRequired);
        var totalPoints = items.Sum(x => x.MaxPoints);
        var earnedPoints = items.Sum(x => x.EarnedPoints);

        return new GetMyProgressResponse
        {
            Summary = $"Выполнено {completed} из {laboratories.TotalCount}, баллов {earnedPoints}",
            TotalLaboratories = laboratories.TotalCount,
            CompletedLaboratories = completed,
            PendingReviewLaboratories = pending,
            RevisionRequiredLaboratories = rejected,
            RejectedLaboratories = rejected,
            TotalPoints = totalPoints,
            EarnedPoints = earnedPoints,
            ProgressPercent = laboratories.TotalCount == 0 ? 0 : completed * 100 / laboratories.TotalCount,
            Laboratories = items
        };
    }

    /// <inheritdoc />
    public async Task<GetMyGradebookResponse?> GetMyGradebookAsync(Guid studentId, CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);

        var student = await connection.QueryFirstOrDefaultAsync<GradebookStudentDto>(
            """
            SELECT
                u.id AS "Id",
                u.full_name AS "FullName",
                g.name AS "GroupName"
            FROM users u
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            LEFT JOIN groups g ON g.id = ug.group_id
            WHERE u.id = @StudentId;
            """,
            new { StudentId = studentId });

        if (student is null)
        {
            return null;
        }

        var record = await connection.QueryFirstOrDefaultAsync<GradebookRecordDbModel>(
            """
            SELECT
                COALESCE(attendance_percent, 0) AS "AttendancePercent",
                COALESCE(is_exam_allowed, false) AS "IsExamAllowed",
                COALESCE(has_automatic_grade, false) AS "HasAutomaticGrade"
            FROM student_gradebook_records
            WHERE student_id = @StudentId;
            """,
            new { StudentId = studentId });

        var laboratories = (await connection.QueryAsync<MyGradebookLaboratoryDto>(
            """
            SELECT
                lw.id AS "LaboratoryId",
                lw.title AS "Title",
                CASE
                    WHEN r.status = 4 THEN 3
                    WHEN lp.status = 3 THEN 3
                    WHEN r.id IS NOT NULL OR lp.id IS NOT NULL THEN 1
                    ELSE 0
                END AS "LaboratoryStatus",
                COALESCE(r.status, 0) AS "Status",
                r.points AS "Points",
                lw.max_points AS "MaxPoints",
                r.teacher_comment AS "TeacherComment"
            FROM laboratory_works lw
            LEFT JOIN laboratory_reports r ON r.laboratory_work_id = lw.id AND r.student_id = @StudentId
            LEFT JOIN laboratory_progress lp ON lp.laboratory_work_id = lw.id AND lp.student_id = @StudentId
            WHERE lw.is_published = true AND lw.delete_date_utc IS NULL
            ORDER BY lw.sort_order, lw.title;
            """,
            new { StudentId = studentId })).ToList();

        return new GetMyGradebookResponse
        {
            Student = student,
            AttendancePercent = record?.AttendancePercent ?? 0,
            IsExamAllowed = record?.IsExamAllowed ?? false,
            HasAutomaticGrade = record?.HasAutomaticGrade ?? false,
            TotalPoints = laboratories
                .Where(x => x.Status == LaboratoryReportStatus.Accepted)
                .Sum(x => x.Points ?? 0),
            Laboratories = laboratories
        };
    }

    /// <inheritdoc />
    public async Task<GetGroupLeaderboardResponse> GetGroupLeaderboardAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           WITH group_id_cte AS (
                               SELECT ug.group_id
                               FROM user_groups ug
                               WHERE ug.user_id = @StudentId
                               LIMIT 1
                           ),
                           group_students AS (
                               SELECT u.id, u.full_name
                               FROM users u
                               JOIN user_groups ug ON ug.user_id = u.id
                               JOIN group_id_cte g ON g.group_id = ug.group_id
                               WHERE u.role = 0
                           ),
                           penalties AS (
                               SELECT sh.student_id, sh.laboratory_work_id,
                                      COALESCE(SUM(h.penalty_points), 0)::int AS penalty_pts
                               FROM student_laboratory_hints sh
                               JOIN laboratory_hints h ON h.id = sh.laboratory_hint_id
                               WHERE sh.student_id IN (SELECT id FROM group_students)
                               GROUP BY sh.student_id, sh.laboratory_work_id
                           ),
                           flags AS (
                               SELECT fa.student_id, fa.laboratory_work_id,
                                      BOOL_OR(fa.is_correct) AS is_correct
                               FROM laboratory_flag_attempts fa
                               WHERE fa.student_id IN (SELECT id FROM group_students)
                               GROUP BY fa.student_id, fa.laboratory_work_id
                           ),
                           student_points AS (
                               SELECT
                                   gs.id AS student_id,
                                   gs.full_name,
                                   COALESCE(SUM(
                                       GREATEST(
                                           COALESCE(
                                               CASE WHEN r.status = 4 THEN r.points ELSE NULL END,
                                               CASE WHEN COALESCE(f.is_correct, false)
                                                    THEN lw.max_points - COALESCE(p.penalty_pts, 0)
                                                    ELSE 0 END
                                           ), 0
                                       )
                                   ), 0)::int AS earned_points
                               FROM group_students gs
                               CROSS JOIN laboratory_works lw
                               LEFT JOIN laboratory_reports r
                                   ON r.laboratory_work_id = lw.id AND r.student_id = gs.id
                               LEFT JOIN penalties p
                                   ON p.student_id = gs.id AND p.laboratory_work_id = lw.id
                               LEFT JOIN flags f
                                   ON f.student_id = gs.id AND f.laboratory_work_id = lw.id
                               WHERE lw.is_published = true AND lw.delete_date_utc IS NULL
                               GROUP BY gs.id, gs.full_name
                           ),
                           ranked AS (
                               SELECT
                                   student_id AS "StudentId",
                                   full_name AS "FullName",
                                   earned_points AS "EarnedPoints",
                                   RANK() OVER (ORDER BY earned_points DESC) AS "Rank",
                                   (student_id = @StudentId) AS "IsCurrentUser"
                               FROM student_points
                           )
                           SELECT *
                           FROM ranked
                           ORDER BY "Rank", "FullName";
                           """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        var items = (await connection.QueryAsync<LeaderboardItemDto>(sql, new { StudentId = studentId })).ToList();

        var currentRank = items.FirstOrDefault(x => x.IsCurrentUser)?.Rank ?? 0;

        return new GetGroupLeaderboardResponse
        {
            CurrentUserRank = currentRank,
            Items = items
        };
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<TeacherLaboratoryListItemDto>> GetTeacherLaboratoriesAsync(
        GetTeacherLaboratoryListRequest request,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT COUNT(*)
                           FROM laboratory_works
                           WHERE delete_date_utc IS NULL
                             AND (@IncludeAll = true OR created_by_teacher_id = @TeacherId)
                             AND (@Block IS NULL OR block = @Block)
                             AND (@Difficulty IS NULL OR difficulty = @Difficulty)
                             AND (@IsPublished IS NULL OR is_published = @IsPublished)
                             AND (@Search IS NULL OR LOWER(title) LIKE LOWER('%' || @Search || '%'));

                           SELECT
                               id AS "Id",
                               title AS "Title",
                               short_description AS "ShortDescription",
                               difficulty AS "Difficulty",
                               block AS "Block",
                               max_points AS "MaxPoints",
                               has_flag AS "HasFlag",
                               is_published AS "IsPublished",
                               sort_order AS "SortOrder",
                               create_date_utc AS "CreateDateUtc",
                               update_date_utc AS "UpdateDateUtc"
                           FROM laboratory_works
                           WHERE delete_date_utc IS NULL
                             AND (@IncludeAll = true OR created_by_teacher_id = @TeacherId)
                             AND (@Block IS NULL OR block = @Block)
                             AND (@Difficulty IS NULL OR difficulty = @Difficulty)
                             AND (@IsPublished IS NULL OR is_published = @IsPublished)
                             AND (@Search IS NULL OR LOWER(title) LIKE LOWER('%' || @Search || '%'))
                           ORDER BY sort_order, title
                           OFFSET @Offset LIMIT @PageSize;
                           """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(sql, new
        {
            request.Block,
            Difficulty = (int?)request.Difficulty,
            request.IsPublished,
            request.Search,
            TeacherId = teacherId,
            IncludeAll = includeAll,
            Offset = (request.Page - 1) * request.PageSize,
            request.PageSize
        });

        var totalCount = await grid.ReadSingleAsync<int>();
        var items = (await grid.ReadAsync<TeacherLaboratoryListItemDto>()).ToList();

        return new PagedResultDto<TeacherLaboratoryListItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<GetTeacherLaboratoryDetailsResponse?> GetTeacherLaboratoryDetailsAsync(
        Guid laboratoryId,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);

        var laboratory = await connection.QueryFirstOrDefaultAsync<GetTeacherLaboratoryDetailsResponse>(
            """
            SELECT
                id AS "Id",
                title AS "Title",
                short_description AS "ShortDescription",
                description AS "Description",
                narrative AS "Narrative",
                goal AS "Goal",
                environment_url AS "EnvironmentUrl",
                credentials AS "Credentials",
                difficulty AS "Difficulty",
                block AS "Block",
                max_points AS "MaxPoints",
                has_flag AS "HasFlag",
                CASE WHEN expected_flag_hash IS NULL THEN false ELSE true END AS "HasExpectedFlag",
                is_published AS "IsPublished",
                sort_order AS "SortOrder",
                create_date_utc AS "CreateDateUtc",
                update_date_utc AS "UpdateDateUtc",
                delete_date_utc AS "DeleteDateUtc"
            FROM laboratory_works
            WHERE id = @LaboratoryId
              AND (@IncludeAll = true OR created_by_teacher_id = @TeacherId)
            LIMIT 1;
            """,
            new { LaboratoryId = laboratoryId, TeacherId = teacherId, IncludeAll = includeAll });

        if (laboratory is null)
        {
            return null;
        }

        var hints = (await connection.QueryAsync<LaboratoryHintInputDto>(
            """
            SELECT
                id AS "Id",
                order_number AS "OrderNumber",
                title AS "Title",
                text AS "Text",
                penalty_points AS "PenaltyPoints"
            FROM laboratory_hints
            WHERE laboratory_work_id = @LaboratoryId
            ORDER BY order_number;
            """,
            new { LaboratoryId = laboratoryId })).ToList();

        return laboratory with { Hints = hints };
    }

    /// <inheritdoc />
    public Task<string?> GetExpectedFlagHashAsync(Guid laboratoryId, CancellationToken cancellationToken)
    {
        return _connection.QueryFirstOrDefaultAsync<string>(
            """
            SELECT expected_flag_hash
            FROM laboratory_works
            WHERE id = @LaboratoryId;
            """,
            new { LaboratoryId = laboratoryId },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Guid> CreateLaboratoryAsync(
        CreateLaboratoryRequest request,
        string? expectedFlagHash,
        Guid teacherId,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var laboratoryId = UUIDNext.Uuid.NewSequential();
        var nowUtc = DateTimeOffset.UtcNow;

        await connection.ExecuteAsync(
            """
            INSERT INTO laboratory_works (
                id, title, short_description, description, narrative, goal,
                environment_url, credentials, difficulty, block, max_points,
                has_flag, expected_flag_hash, created_by_teacher_id, is_published, sort_order,
                create_date_utc, update_date_utc, delete_date_utc
            )
            VALUES (
                @Id, @Title, @ShortDescription, @Description, @Narrative, @Goal,
                @EnvironmentUrl, @Credentials, @Difficulty, @Block, @MaxPoints,
                @HasFlag, @ExpectedFlagHash, @TeacherId, @IsPublished, @SortOrder,
                @NowUtc, NULL, NULL
            );
            """,
            new
            {
                Id = laboratoryId,
                request.Title,
                request.ShortDescription,
                request.Description,
                request.Narrative,
                request.Goal,
                request.EnvironmentUrl,
                request.Credentials,
                Difficulty = (int)request.Difficulty,
                request.Block,
                request.MaxPoints,
                request.HasFlag,
                ExpectedFlagHash = expectedFlagHash,
                TeacherId = teacherId,
                request.IsPublished,
                request.SortOrder,
                NowUtc = nowUtc
            },
            transaction);

        await InsertHintsAsync(connection, transaction, laboratoryId, request.Hints, nowUtc);

        await transaction.CommitAsync(cancellationToken);

        return laboratoryId;
    }

    /// <inheritdoc />
    public async Task UpdateLaboratoryAsync(
        Guid laboratoryId,
        UpdateLaboratoryRequest request,
        string? expectedFlagHash,
        bool updateFlagHash,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var exists = await connection.QuerySingleAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM laboratory_works
                WHERE id = @LaboratoryId
                  AND delete_date_utc IS NULL
                  AND (@IncludeAll = true OR created_by_teacher_id = @TeacherId)
            );
            """,
            new { LaboratoryId = laboratoryId, TeacherId = teacherId, IncludeAll = includeAll },
            transaction);

        if (!exists)
        {
            throw new LaboratoryException("laboratory.not_found", "Лабораторная работа не найдена");
        }

        var nowUtc = DateTimeOffset.UtcNow;

        await connection.ExecuteAsync(
            """
            UPDATE laboratory_works
            SET title = @Title,
                short_description = @ShortDescription,
                description = @Description,
                narrative = @Narrative,
                goal = @Goal,
                environment_url = @EnvironmentUrl,
                credentials = @Credentials,
                difficulty = @Difficulty,
                block = @Block,
                max_points = @MaxPoints,
                has_flag = @HasFlag,
                expected_flag_hash = CASE
                    WHEN @HasFlag = false THEN NULL
                    WHEN @UpdateFlagHash = true THEN @ExpectedFlagHash
                    ELSE expected_flag_hash
                END,
                is_published = @IsPublished,
                sort_order = @SortOrder,
                update_date_utc = @NowUtc
            WHERE id = @LaboratoryId;
            """,
            new
            {
                LaboratoryId = laboratoryId,
                request.Title,
                request.ShortDescription,
                request.Description,
                request.Narrative,
                request.Goal,
                request.EnvironmentUrl,
                request.Credentials,
                Difficulty = (int)request.Difficulty,
                request.Block,
                request.MaxPoints,
                request.HasFlag,
                ExpectedFlagHash = expectedFlagHash,
                UpdateFlagHash = updateFlagHash,
                request.IsPublished,
                request.SortOrder,
                NowUtc = nowUtc
            },
            transaction);

        var keptHintIds = request.Hints
            .Where(x => x.Id.HasValue)
            .Select(x => x.Id!.Value)
            .ToArray();

        await connection.ExecuteAsync(
            """
            DELETE FROM laboratory_hints
            WHERE laboratory_work_id = @LaboratoryId
              AND (array_length(@KeptHintIds, 1) IS NULL OR id <> ALL(@KeptHintIds));
            """,
            new { LaboratoryId = laboratoryId, KeptHintIds = keptHintIds },
            transaction);

        foreach (var hint in request.Hints)
        {
            if (hint.Id.HasValue)
            {
                await connection.ExecuteAsync(
                    """
                    UPDATE laboratory_hints
                    SET order_number = @OrderNumber,
                        title = @Title,
                        text = @Text,
                        penalty_points = @PenaltyPoints,
                        update_date_utc = @NowUtc
                    WHERE id = @Id AND laboratory_work_id = @LaboratoryId;
                    """,
                    new
                    {
                        hint.Id,
                        LaboratoryId = laboratoryId,
                        hint.OrderNumber,
                        hint.Title,
                        hint.Text,
                        hint.PenaltyPoints,
                        NowUtc = nowUtc
                    },
                    transaction);

                continue;
            }

            await InsertHintAsync(connection, transaction, laboratoryId, hint, nowUtc);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteLaboratoryAsync(
        Guid laboratoryId,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        var affected = await _connection.ExecuteAsync(
            """
            UPDATE laboratory_works
            SET delete_date_utc = @DeleteDateUtc,
                is_published = false,
                update_date_utc = @DeleteDateUtc
            WHERE id = @LaboratoryId
              AND (@IncludeAll = true OR created_by_teacher_id = @TeacherId)
              AND delete_date_utc IS NULL;
            """,
            new
            {
                LaboratoryId = laboratoryId,
                TeacherId = teacherId,
                IncludeAll = includeAll,
                DeleteDateUtc = DateTimeOffset.UtcNow
            },
            cancellationToken);

        if (affected == 0)
        {
            throw new LaboratoryException("laboratory.not_found", "Лабораторная работа не найдена");
        }
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<TeacherReportListItemDto>> GetTeacherReportsAsync(
        GetTeacherReportListRequest request,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT COUNT(*)
                           FROM laboratory_reports r
                           JOIN laboratory_works lw ON lw.id = r.laboratory_work_id
                           JOIN users u ON u.id = r.student_id
                           LEFT JOIN user_groups ug ON ug.user_id = u.id
                           LEFT JOIN groups g ON g.id = ug.group_id
                           WHERE (@Status IS NULL OR r.status = @Status)
                             AND (
                                 @IncludeAll = true
                                 OR EXISTS (
                                     SELECT 1
                                     FROM teacher_groups tg
                                     JOIN user_groups sug ON sug.group_id = tg.group_id
                                     WHERE tg.teacher_id = @TeacherId
                                       AND sug.user_id = r.student_id
                                 )
                             )
                             AND (@LaboratoryId IS NULL OR lw.id = @LaboratoryId)
                             AND (@Search IS NULL OR LOWER(u.full_name) LIKE LOWER('%' || @Search || '%'))
                             AND (@GroupName IS NULL OR g.name = @GroupName);

                           SELECT
                               r.id AS "ReportId",
                               lw.id AS "LaboratoryId",
                               lw.title AS "LaboratoryTitle",
                               u.id AS "StudentId",
                               u.full_name AS "StudentFullName",
                               g.name AS "GroupName",
                               r.current_version_number AS "CurrentVersionNumber",
                               r.status AS "Status",
                               r.points AS "Points",
                               lw.max_points AS "MaxPoints",
                               r.allow_resubmit AS "AllowResubmit",
                               r.create_date_utc AS "CreateDateUtc",
                               r.update_date_utc AS "UpdateDateUtc",
                               rv.create_date_utc AS "LastSubmitDateUtc"
                           FROM laboratory_reports r
                           JOIN laboratory_works lw ON lw.id = r.laboratory_work_id
                           JOIN users u ON u.id = r.student_id
                           LEFT JOIN user_groups ug ON ug.user_id = u.id
                           LEFT JOIN groups g ON g.id = ug.group_id
                           JOIN laboratory_report_versions rv ON rv.laboratory_report_id = r.id AND rv.version_number = r.current_version_number
                           WHERE (@Status IS NULL OR r.status = @Status)
                              AND (
                                  @IncludeAll = true
                                  OR EXISTS (
                                      SELECT 1
                                      FROM teacher_groups tg
                                      JOIN user_groups sug ON sug.group_id = tg.group_id
                                      WHERE tg.teacher_id = @TeacherId
                                        AND sug.user_id = r.student_id
                                  )
                              )
                             AND (@LaboratoryId IS NULL OR lw.id = @LaboratoryId)
                             AND (@Search IS NULL OR LOWER(u.full_name) LIKE LOWER('%' || @Search || '%'))
                             AND (@GroupName IS NULL OR g.name = @GroupName)
                           ORDER BY rv.create_date_utc DESC
                           OFFSET @Offset LIMIT @PageSize;
                           """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(sql, new
        {
            Status = (int?)request.Status,
            request.LaboratoryId,
            request.Search,
            request.GroupName,
            TeacherId = teacherId,
            IncludeAll = includeAll,
            Offset = (request.Page - 1) * request.PageSize,
            request.PageSize
        });

        var totalCount = await grid.ReadSingleAsync<int>();
        var items = (await grid.ReadAsync<TeacherReportListItemDto>()).ToList();

        return new PagedResultDto<TeacherReportListItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<GetTeacherReportDetailsResponse?> GetTeacherReportDetailsAsync(
        Guid reportId,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);

        var report = await connection.QueryFirstOrDefaultAsync<GetTeacherReportDetailsResponse>(
            """
            SELECT
                r.id AS "ReportId",
                r.status AS "Status",
                r.points AS "Points",
                r.teacher_comment AS "TeacherComment",
                r.allow_resubmit AS "AllowResubmit"
            FROM laboratory_reports r
            JOIN laboratory_works lw ON lw.id = r.laboratory_work_id
            WHERE r.id = @ReportId
              AND (
                  @IncludeAll = true
                  OR EXISTS (
                      SELECT 1
                      FROM teacher_groups tg
                      JOIN user_groups sug ON sug.group_id = tg.group_id
                      WHERE tg.teacher_id = @TeacherId
                        AND sug.user_id = r.student_id
                  )
              );
            """,
            new { ReportId = reportId, TeacherId = teacherId, IncludeAll = includeAll });

        if (report is null)
        {
            return null;
        }

        var laboratory = await connection.QuerySingleAsync<TeacherReportLaboratoryDto>(
            """
            SELECT
                lw.id AS "Id",
                lw.title AS "Title",
                lw.max_points AS "MaxPoints"
            FROM laboratory_reports r
            JOIN laboratory_works lw ON lw.id = r.laboratory_work_id
            WHERE r.id = @ReportId;
            """,
            new { ReportId = reportId });

        var student = await connection.QuerySingleAsync<GradebookStudentDto>(
            """
            SELECT
                u.id AS "Id",
                u.full_name AS "FullName",
                g.name AS "GroupName"
            FROM laboratory_reports r
            JOIN users u ON u.id = r.student_id
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            LEFT JOIN groups g ON g.id = ug.group_id
            WHERE r.id = @ReportId;
            """,
            new { ReportId = reportId });

        var versions = (await connection.QueryAsync<LaboratoryReportVersionDto>(
            """
            SELECT
                rv.id AS "VersionId",
                rv.version_number AS "VersionNumber",
                rv.original_file_name AS "OriginalFileName",
                rv.file_size_bytes AS "FileSizeBytes",
                rv.content_type AS "ContentType",
                rv.status AS "Status",
                rv.points AS "Points",
                rv.teacher_comment AS "TeacherComment",
                rv.create_date_utc AS "CreateDateUtc",
                rv.checked_by_teacher_id AS "CheckedByTeacherId",
                t.full_name AS "CheckedByTeacherFullName",
                rv.checked_date_utc AS "CheckedDateUtc",
                '/public/api/v1/teacher/reports/' || @ReportId || '/versions/' || rv.id || '/file' AS "FileDownloadUrl"
            FROM laboratory_report_versions rv
            LEFT JOIN users t ON t.id = rv.checked_by_teacher_id
            WHERE rv.laboratory_report_id = @ReportId
            ORDER BY rv.version_number DESC;
            """,
            new { ReportId = reportId })).ToList();

        return report with
        {
            Laboratory = laboratory,
            Student = student,
            Versions = versions
        };
    }

    /// <inheritdoc />
    public Task<LaboratoryReportFileDto?> GetReportFileAsync(
        Guid reportId,
        Guid versionId,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        return _connection.QueryFirstOrDefaultAsync<LaboratoryReportFileDto>(
            """
            SELECT
                rv.storage_path AS "StoragePath",
                rv.original_file_name AS "OriginalFileName",
                rv.content_type AS "ContentType"
            FROM laboratory_report_versions rv
            JOIN laboratory_reports r ON r.id = rv.laboratory_report_id
            JOIN laboratory_works lw ON lw.id = r.laboratory_work_id
            WHERE rv.id = @VersionId
              AND rv.laboratory_report_id = @ReportId
              AND (
                  @IncludeAll = true
                  OR EXISTS (
                      SELECT 1
                      FROM teacher_groups tg
                      JOIN user_groups sug ON sug.group_id = tg.group_id
                      WHERE tg.teacher_id = @TeacherId
                        AND sug.user_id = r.student_id
                  )
              );
            """,
            new { ReportId = reportId, VersionId = versionId, TeacherId = teacherId, IncludeAll = includeAll },
            cancellationToken);
    }

    /// <inheritdoc />
    public Task<LaboratoryReportFileDto?> GetStudentReportFileAsync(
        Guid studentId,
        Guid laboratoryId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        return _connection.QueryFirstOrDefaultAsync<LaboratoryReportFileDto>(
            """
            SELECT
                rv.storage_path AS "StoragePath",
                rv.original_file_name AS "OriginalFileName",
                rv.content_type AS "ContentType"
            FROM laboratory_report_versions rv
            JOIN laboratory_reports r ON r.id = rv.laboratory_report_id
            WHERE rv.id = @VersionId
              AND r.student_id = @StudentId
              AND r.laboratory_work_id = @LaboratoryId;
            """,
            new { VersionId = versionId, StudentId = studentId, LaboratoryId = laboratoryId },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ReviewLaboratoryReportResponse> ReviewReportAsync(
        Guid teacherId,
        bool includeAll,
        Guid reportId,
        ReviewLaboratoryReportRequest request,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var report = await connection.QueryFirstOrDefaultAsync<ReviewReportDbModel>(
            """
            SELECT
                r.id AS "ReportId",
                r.student_id AS "StudentId",
                r.laboratory_work_id AS "LaboratoryId",
                r.current_version_number AS "CurrentVersionNumber",
                r.status AS "Status",
                lw.max_points AS "MaxPoints"
            FROM laboratory_reports r
            JOIN laboratory_works lw ON lw.id = r.laboratory_work_id
            WHERE r.id = @ReportId
              AND (
                  @IncludeAll = true
                  OR EXISTS (
                      SELECT 1
                      FROM teacher_groups tg
                      JOIN user_groups sug ON sug.group_id = tg.group_id
                      WHERE tg.teacher_id = @TeacherId
                        AND sug.user_id = r.student_id
                  )
              )
            FOR UPDATE;
            """,
            new { ReportId = reportId, TeacherId = teacherId, IncludeAll = includeAll },
            transaction);

        if (report is null)
        {
            throw new LaboratoryException("laboratory_report.not_found", "Отчет не найден");
        }

        if (request.Points > report.MaxPoints)
        {
            throw new LaboratoryException("laboratory_review.points_out_of_range", "Баллы не могут быть больше максимального балла");
        }

        if (report.Status is not LaboratoryReportStatus.Submitted and not LaboratoryReportStatus.UnderReview)
        {
            throw new LaboratoryException("laboratory_report.not_pending_review", "Проверить можно только отчет в статусе ожидания проверки");
        }

        var version = await connection.QueryFirstOrDefaultAsync<Guid?>(
            """
            SELECT id
            FROM laboratory_report_versions
            WHERE laboratory_report_id = @ReportId AND version_number = @VersionNumber
            FOR UPDATE;
            """,
            new { ReportId = reportId, VersionNumber = report.CurrentVersionNumber },
            transaction);

        if (!version.HasValue)
        {
            throw new LaboratoryException("laboratory_report_version.not_found", "Версия отчета не найдена");
        }

        var nowUtc = DateTimeOffset.UtcNow;
        var points = request.Status == LaboratoryReportStatus.UnderReview ? null : request.Points;
        var allowResubmit = request.Status == LaboratoryReportStatus.Accepted ? false : request.AllowResubmit;

        await connection.ExecuteAsync(
            """
            UPDATE laboratory_report_versions
            SET status = @Status,
                points = @Points,
                teacher_comment = @Comment,
                checked_by_teacher_id = @TeacherId,
                checked_date_utc = @NowUtc,
                allow_resubmit_after_review = @AllowResubmit
            WHERE id = @VersionId;

            UPDATE laboratory_reports
            SET status = @Status,
                points = @Points,
                teacher_comment = @Comment,
                allow_resubmit = @AllowResubmit,
                update_date_utc = @NowUtc
            WHERE id = @ReportId;
            """,
            new
            {
                VersionId = version.Value,
                ReportId = reportId,
                Status = (int)request.Status,
                Points = points,
                Comment = request.Comment,
                TeacherId = teacherId,
                NowUtc = nowUtc,
                AllowResubmit = allowResubmit
            },
            transaction);

        await connection.ExecuteAsync(
            """
            INSERT INTO laboratory_progress (
                id, laboratory_work_id, student_id, status, started_at_utc, completed_at_utc
            )
            VALUES (
                @Id, @LaboratoryId, @StudentId, @Status, @StartedAtUtc, @CompletedAtUtc
            )
            ON CONFLICT (laboratory_work_id, student_id) DO UPDATE
            SET status = EXCLUDED.status,
                completed_at_utc = EXCLUDED.completed_at_utc;
            """,
            new
            {
                Id = UUIDNext.Uuid.NewSequential(),
                report.LaboratoryId,
                report.StudentId,
                Status = request.Status == LaboratoryReportStatus.Accepted
                    ? (int)StudentLaboratoryStatus.Accepted
                    : (int)StudentLaboratoryStatus.InProgress,
                StartedAtUtc = nowUtc,
                CompletedAtUtc = request.Status == LaboratoryReportStatus.Accepted ? nowUtc : (DateTimeOffset?)null
            },
            transaction);

        await transaction.CommitAsync(cancellationToken);

        return new ReviewLaboratoryReportResponse
        {
            ReportId = reportId,
            StudentId = report.StudentId,
            Status = request.Status,
            Points = points,
            TeacherComment = request.Comment,
            AllowResubmit = allowResubmit,
            CheckedDateUtc = nowUtc
        };
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<TeacherGradebookItemDto>> GetTeacherGradebookAsync(
        GetTeacherGradebookRequest request,
        Guid teacherId,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        const string sql = """
                           SELECT COUNT(*)
                           FROM users u
                           LEFT JOIN user_groups ug ON ug.user_id = u.id
                           LEFT JOIN groups g ON g.id = ug.group_id
                           LEFT JOIN student_gradebook_records sgr ON sgr.student_id = u.id
                           WHERE u.role = @StudentRole
                             AND (
                                 @IncludeAll = true
                                 OR EXISTS (
                                     SELECT 1
                                     FROM teacher_groups tg
                                     WHERE tg.teacher_id = @TeacherId
                                       AND tg.group_id = ug.group_id
                                 )
                             )
                             AND (@GroupName IS NULL OR g.name = @GroupName)
                             AND (@IsExamAllowed IS NULL OR COALESCE(sgr.is_exam_allowed, false) = @IsExamAllowed)
                             AND (@Search IS NULL OR LOWER(u.full_name) LIKE LOWER('%' || @Search || '%'));

                           SELECT
                               u.id AS "StudentId",
                               u.full_name AS "FullName",
                               g.name AS "GroupName",
                               COALESCE(sgr.attendance_percent, 0) AS "AttendancePercent",
                               COALESCE(sgr.is_exam_allowed, false) AS "IsExamAllowed",
                               COALESCE(sgr.has_automatic_grade, false) AS "HasAutomaticGrade",
                               COALESCE(SUM(CASE WHEN r.status = 4 THEN r.points ELSE 0 END), 0)::int AS "TotalPoints",
                               COUNT(CASE WHEN r.status = 4 THEN 1 END)::int AS "CompletedLaboratories",
                               (
                                   SELECT COUNT(*)::int
                                   FROM laboratory_works
                                   WHERE is_published = true AND delete_date_utc IS NULL
                               ) AS "TotalLaboratories"
                           FROM users u
                           LEFT JOIN user_groups ug ON ug.user_id = u.id
                           LEFT JOIN groups g ON g.id = ug.group_id
                           LEFT JOIN student_gradebook_records sgr ON sgr.student_id = u.id
                           LEFT JOIN laboratory_reports r ON r.student_id = u.id
                           WHERE u.role = @StudentRole
                             AND (
                                 @IncludeAll = true
                                 OR EXISTS (
                                     SELECT 1
                                     FROM teacher_groups tg
                                     WHERE tg.teacher_id = @TeacherId
                                       AND tg.group_id = ug.group_id
                                 )
                             )
                             AND (@GroupName IS NULL OR g.name = @GroupName)
                             AND (@IsExamAllowed IS NULL OR COALESCE(sgr.is_exam_allowed, false) = @IsExamAllowed)
                             AND (@Search IS NULL OR LOWER(u.full_name) LIKE LOWER('%' || @Search || '%'))
                           GROUP BY u.id, u.full_name, g.name, sgr.attendance_percent, sgr.is_exam_allowed, sgr.has_automatic_grade
                           ORDER BY g.name, u.full_name
                           OFFSET @Offset LIMIT @PageSize;
                           """;

        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);
        using var grid = await connection.QueryMultipleAsync(sql, new
        {
            StudentRole = (int)UserRole.Student,
            request.GroupName,
            request.IsExamAllowed,
            request.Search,
            TeacherId = teacherId,
            IncludeAll = includeAll,
            Offset = (request.Page - 1) * request.PageSize,
            request.PageSize
        });

        var totalCount = await grid.ReadSingleAsync<int>();
        var items = (await grid.ReadAsync<TeacherGradebookItemDto>()).ToList();

        return new PagedResultDto<TeacherGradebookItemDto>(items, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<TeacherGradebookItemDto> UpdateGradebookAsync(
        Guid teacherId,
        Guid studentId,
        UpdateTeacherGradebookRequest request,
        bool includeAll,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connection.CreateConnectionAsync(cancellationToken);

        var hasAccess = await connection.QuerySingleAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1
                FROM users u
                LEFT JOIN user_groups ug ON ug.user_id = u.id
                WHERE u.id = @StudentId
                  AND u.role = @StudentRole
                  AND (
                      @IncludeAll = true
                      OR EXISTS (
                          SELECT 1
                          FROM teacher_groups tg
                          WHERE tg.teacher_id = @TeacherId
                            AND tg.group_id = ug.group_id
                      )
                  )
            );
            """,
            new
            {
                StudentId = studentId,
                StudentRole = (int)UserRole.Student,
                TeacherId = teacherId,
                IncludeAll = includeAll
            },
            commandTimeout: null,
            commandType: null);

        if (!hasAccess)
        {
            throw new LaboratoryException("gradebook.not_found", "Запись ведомости не найдена");
        }

        await connection.ExecuteAsync(
            """
            INSERT INTO student_gradebook_records (
                id, student_id, group_id, attendance_percent, is_exam_allowed,
                has_automatic_grade, update_date_utc, updated_by_teacher_id
            )
            SELECT
                @Id, @StudentId, ug.group_id, @AttendancePercent, @IsExamAllowed,
                @HasAutomaticGrade, @UpdateDateUtc, @TeacherId
            FROM users u
            LEFT JOIN user_groups ug ON ug.user_id = u.id
            WHERE u.id = @StudentId
            ORDER BY ug.group_id NULLS LAST
            LIMIT 1
            ON CONFLICT (student_id) DO UPDATE
            SET attendance_percent = EXCLUDED.attendance_percent,
                is_exam_allowed = EXCLUDED.is_exam_allowed,
                has_automatic_grade = EXCLUDED.has_automatic_grade,
                update_date_utc = EXCLUDED.update_date_utc,
                updated_by_teacher_id = EXCLUDED.updated_by_teacher_id;
            """,
            new
            {
                Id = UUIDNext.Uuid.NewSequential(),
                StudentId = studentId,
                request.AttendancePercent,
                request.IsExamAllowed,
                request.HasAutomaticGrade,
                UpdateDateUtc = DateTimeOffset.UtcNow,
                TeacherId = teacherId
            });

        var gradebook = await GetTeacherGradebookAsync(
            new GetTeacherGradebookRequest { Search = null, Page = 1, PageSize = 100 },
            teacherId,
            includeAll,
            cancellationToken);

        return gradebook.Items.FirstOrDefault(x => x.StudentId == studentId)
               ?? throw new LaboratoryException("gradebook.not_found", "Запись ведомости не найдена");
    }

    private static async Task InsertHintsAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        Guid laboratoryId,
        IEnumerable<LaboratoryHintInputDto> hints,
        DateTimeOffset nowUtc)
    {
        foreach (var hint in hints)
        {
            await InsertHintAsync(connection, transaction, laboratoryId, hint, nowUtc);
        }
    }

    private static Task EnsureProgressStartedAsync(
        Npgsql.NpgsqlConnection connection,
        Guid studentId,
        Guid laboratoryId,
        System.Data.Common.DbTransaction? transaction = null)
    {
        return connection.ExecuteAsync(
            """
            INSERT INTO laboratory_progress (
                id, laboratory_work_id, student_id, status, started_at_utc, completed_at_utc
            )
            VALUES (
                @Id, @LaboratoryId, @StudentId, @Status, @StartedAtUtc, NULL
            )
            ON CONFLICT (laboratory_work_id, student_id) DO NOTHING;
            """,
            new
            {
                Id = UUIDNext.Uuid.NewSequential(),
                LaboratoryId = laboratoryId,
                StudentId = studentId,
                Status = (int)StudentLaboratoryStatus.InProgress,
                StartedAtUtc = DateTimeOffset.UtcNow
            },
            transaction);
    }

    private static Task InsertHintAsync(
        Npgsql.NpgsqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        Guid laboratoryId,
        LaboratoryHintInputDto hint,
        DateTimeOffset nowUtc)
    {
        return connection.ExecuteAsync(
            """
            INSERT INTO laboratory_hints (
                id, laboratory_work_id, order_number, title, text,
                penalty_points, create_date_utc, update_date_utc
            )
            VALUES (
                @Id, @LaboratoryId, @OrderNumber, @Title, @Text,
                @PenaltyPoints, @NowUtc, NULL
            );
            """,
            new
            {
                Id = hint.Id ?? UUIDNext.Uuid.NewSequential(),
                LaboratoryId = laboratoryId,
                hint.OrderNumber,
                hint.Title,
                hint.Text,
                hint.PenaltyPoints,
                NowUtc = nowUtc
            },
            transaction);
    }

    private static async Task<GetMyLaboratoryReportResponse?> GetMyReportInternalAsync(
        Npgsql.NpgsqlConnection connection,
        Guid studentId,
        Guid laboratoryId)
    {
        var report = await connection.QueryFirstOrDefaultAsync<GetMyLaboratoryReportResponse>(
            """
            SELECT
                id AS "ReportId",
                status AS "Status",
                points AS "Points",
                teacher_comment AS "TeacherComment",
                allow_resubmit AS "AllowResubmit"
            FROM laboratory_reports
            WHERE student_id = @StudentId AND laboratory_work_id = @LaboratoryId;
            """,
            new { StudentId = studentId, LaboratoryId = laboratoryId });

        if (report is null)
        {
            return null;
        }

        var versions = (await connection.QueryAsync<LaboratoryReportVersionDto>(
            """
            SELECT
                id AS "VersionId",
                version_number AS "VersionNumber",
                original_file_name AS "OriginalFileName",
                file_size_bytes AS "FileSizeBytes",
                content_type AS "ContentType",
                status AS "Status",
                points AS "Points",
                teacher_comment AS "TeacherComment",
                create_date_utc AS "CreateDateUtc",
                checked_by_teacher_id AS "CheckedByTeacherId",
                NULL AS "CheckedByTeacherFullName",
                checked_date_utc AS "CheckedDateUtc",
                '/public/api/v1/laboratories/' || @LaboratoryId || '/reports/my/versions/' || id || '/file' AS "FileDownloadUrl"
            FROM laboratory_report_versions
            WHERE laboratory_report_id = @ReportId
            ORDER BY version_number DESC;
            """,
            new { report.ReportId, LaboratoryId = laboratoryId })).ToList();

        return report with { Versions = versions };
    }

    private static string GetDifficultyName(LaboratoryDifficulty difficulty)
    {
        return difficulty switch
        {
            LaboratoryDifficulty.Easy => "Легкая",
            LaboratoryDifficulty.Medium => "Средняя",
            LaboratoryDifficulty.Hard => "Сложная",
            _ => difficulty.ToString()
        };
    }

    private static string GetStudentStatusName(StudentLaboratoryStatus status)
    {
        return status switch
        {
            StudentLaboratoryStatus.NotStarted => "Не начата",
            StudentLaboratoryStatus.InProgress => "В работе",
            StudentLaboratoryStatus.PendingReview => "Ожидает проверки",
            StudentLaboratoryStatus.Accepted => "Принята",
            StudentLaboratoryStatus.RevisionRequired => "Нужна доработка",
            _ => status.ToString()
        };
    }

    private sealed record ReportStateDbModel
    {
        public Guid Id { get; init; }
        public LaboratoryReportStatus Status { get; init; }
        public int CurrentVersionNumber { get; init; }
        public bool AllowResubmit { get; init; }
    }

    private sealed record ReviewReportDbModel
    {
        public Guid ReportId { get; init; }
        public Guid StudentId { get; init; }
        public Guid LaboratoryId { get; init; }
        public int CurrentVersionNumber { get; init; }
        public LaboratoryReportStatus Status { get; init; }
        public int MaxPoints { get; init; }
    }

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Guid>> GetStudentIdsForTeacherAsync(
        Guid teacherId,
        CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT DISTINCT ug.user_id
                               FROM teacher_groups tg
                               JOIN user_groups ug ON ug.group_id = tg.group_id
                               JOIN users u ON u.id = ug.user_id AND u.role = @StudentRole
                               WHERE tg.teacher_id = @TeacherId
                           """;

        return _connection.QueryAsync<Guid>(sql, new { TeacherId = teacherId, StudentRole = (int)UserRole.Student }, cancellationToken);
    }

    private sealed record GradebookRecordDbModel
    {
        public decimal AttendancePercent { get; init; }
        public bool IsExamAllowed { get; init; }
        public bool HasAutomaticGrade { get; init; }
    }
}
