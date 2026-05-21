using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция связей преподавателей с группами и прогресса лабораторных работ
/// </summary>
internal sealed class CreateTeacherGroupsAndLaboratoryProgress_202605210001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateTeacherGroupsAndLaboratoryProgress_202605210001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateTeacherGroupsAndLaboratoryProgress_202605210001);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                           CREATE TABLE IF NOT EXISTS teacher_groups (
                               teacher_id UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
                               group_id UUID NOT NULL REFERENCES groups (id) ON DELETE CASCADE,
                               added_at_utc TIMESTAMPTZ NOT NULL,
                               PRIMARY KEY (teacher_id, group_id)
                           );

                           INSERT INTO teacher_groups (teacher_id, group_id, added_at_utc)
                           SELECT DISTINCT ug.user_id, ug.group_id, NOW()
                           FROM user_groups ug
                           JOIN users u ON u.id = ug.user_id
                           WHERE u.role = 1
                           ON CONFLICT (teacher_id, group_id) DO NOTHING;

                           CREATE TABLE IF NOT EXISTS laboratory_progress (
                               id UUID PRIMARY KEY,
                               laboratory_work_id UUID NOT NULL REFERENCES laboratory_works (id) ON DELETE CASCADE,
                               student_id UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
                               status INTEGER NOT NULL,
                               started_at_utc TIMESTAMPTZ NOT NULL,
                               completed_at_utc TIMESTAMPTZ NULL
                           );

                           CREATE UNIQUE INDEX IF NOT EXISTS ux_laboratory_progress_laboratory_work_id_student_id
                           ON laboratory_progress (laboratory_work_id, student_id);

                           CREATE INDEX IF NOT EXISTS ix_laboratory_progress_student_id
                           ON laboratory_progress (student_id);

                           UPDATE laboratory_reports
                           SET status = 4
                           WHERE status = 2;

                           UPDATE laboratory_report_versions
                           SET status = 4
                           WHERE status = 2;

                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                           DROP TABLE IF EXISTS laboratory_progress;
                           DROP TABLE IF EXISTS teacher_groups;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}
