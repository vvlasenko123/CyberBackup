using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция создания таблиц лабораторных работ
/// </summary>
internal sealed class CreateZLaboratories_202605130001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateZLaboratories_202605130001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(CreateZLaboratories_202605130001);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                           CREATE TABLE IF NOT EXISTS laboratory_works (
                               id UUID PRIMARY KEY,
                               title VARCHAR(255) NOT NULL,
                               short_description VARCHAR(1000) NOT NULL,
                               description TEXT NOT NULL,
                               narrative TEXT NOT NULL,
                               goal TEXT NOT NULL,
                               environment_url TEXT NULL,
                               credentials TEXT NULL,
                               difficulty INTEGER NOT NULL,
                               block VARCHAR(255) NOT NULL,
                               max_points INTEGER NOT NULL,
                               has_flag BOOLEAN NOT NULL,
                               expected_flag_hash VARCHAR(255) NULL,
                               is_published BOOLEAN NOT NULL,
                               sort_order INTEGER NOT NULL,
                               create_date_utc TIMESTAMPTZ NOT NULL,
                               update_date_utc TIMESTAMPTZ NULL,
                               delete_date_utc TIMESTAMPTZ NULL
                           );

                           CREATE INDEX IF NOT EXISTS ix_laboratory_works_is_published ON laboratory_works (is_published);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_works_block ON laboratory_works (block);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_works_difficulty ON laboratory_works (difficulty);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_works_sort_order ON laboratory_works (sort_order);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_works_delete_date_utc ON laboratory_works (delete_date_utc);

                           CREATE TABLE IF NOT EXISTS laboratory_hints (
                               id UUID PRIMARY KEY,
                               laboratory_work_id UUID NOT NULL REFERENCES laboratory_works (id) ON DELETE CASCADE,
                               order_number INTEGER NOT NULL,
                               title VARCHAR(255) NULL,
                               text TEXT NOT NULL,
                               penalty_points INTEGER NOT NULL,
                               create_date_utc TIMESTAMPTZ NOT NULL,
                               update_date_utc TIMESTAMPTZ NULL
                           );

                           CREATE INDEX IF NOT EXISTS ix_laboratory_hints_laboratory_work_id ON laboratory_hints (laboratory_work_id);
                           CREATE UNIQUE INDEX IF NOT EXISTS ux_laboratory_hints_laboratory_work_id_order_number ON laboratory_hints (laboratory_work_id, order_number);

                           CREATE TABLE IF NOT EXISTS student_laboratory_hints (
                               id UUID PRIMARY KEY,
                               student_id UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
                               laboratory_work_id UUID NOT NULL REFERENCES laboratory_works (id) ON DELETE CASCADE,
                               laboratory_hint_id UUID NOT NULL REFERENCES laboratory_hints (id) ON DELETE CASCADE,
                               open_date_utc TIMESTAMPTZ NOT NULL
                           );

                           CREATE UNIQUE INDEX IF NOT EXISTS ux_student_laboratory_hints_student_id_laboratory_hint_id ON student_laboratory_hints (student_id, laboratory_hint_id);
                           CREATE INDEX IF NOT EXISTS ix_student_laboratory_hints_student_id_laboratory_work_id ON student_laboratory_hints (student_id, laboratory_work_id);

                           CREATE TABLE IF NOT EXISTS laboratory_reports (
                               id UUID PRIMARY KEY,
                               student_id UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
                               laboratory_work_id UUID NOT NULL REFERENCES laboratory_works (id) ON DELETE CASCADE,
                               status INTEGER NOT NULL,
                               current_version_number INTEGER NOT NULL,
                               points INTEGER NULL,
                               teacher_comment TEXT NULL,
                               allow_resubmit BOOLEAN NOT NULL,
                               create_date_utc TIMESTAMPTZ NOT NULL,
                               update_date_utc TIMESTAMPTZ NULL
                           );

                           CREATE UNIQUE INDEX IF NOT EXISTS ux_laboratory_reports_student_id_laboratory_work_id ON laboratory_reports (student_id, laboratory_work_id);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_reports_laboratory_work_id ON laboratory_reports (laboratory_work_id);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_reports_status ON laboratory_reports (status);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_reports_create_date_utc ON laboratory_reports (create_date_utc);

                           CREATE TABLE IF NOT EXISTS laboratory_report_versions (
                               id UUID PRIMARY KEY,
                               laboratory_report_id UUID NOT NULL REFERENCES laboratory_reports (id) ON DELETE CASCADE,
                               version_number INTEGER NOT NULL,
                               storage_path TEXT NOT NULL,
                               original_file_name VARCHAR(500) NOT NULL,
                               content_type VARCHAR(255) NOT NULL,
                               file_size_bytes BIGINT NOT NULL,
                               status INTEGER NOT NULL,
                               points INTEGER NULL,
                               teacher_comment TEXT NULL,
                               checked_by_teacher_id UUID NULL REFERENCES users (id) ON DELETE SET NULL,
                               checked_date_utc TIMESTAMPTZ NULL,
                               allow_resubmit_after_review BOOLEAN NULL,
                               create_date_utc TIMESTAMPTZ NOT NULL
                           );

                           CREATE UNIQUE INDEX IF NOT EXISTS ux_laboratory_report_versions_report_id_version_number ON laboratory_report_versions (laboratory_report_id, version_number);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_report_versions_status ON laboratory_report_versions (status);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_report_versions_create_date_utc ON laboratory_report_versions (create_date_utc);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_report_versions_checked_by_teacher_id ON laboratory_report_versions (checked_by_teacher_id);

                           CREATE TABLE IF NOT EXISTS laboratory_flag_attempts (
                               id UUID PRIMARY KEY,
                               student_id UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
                               laboratory_work_id UUID NOT NULL REFERENCES laboratory_works (id) ON DELETE CASCADE,
                               submitted_flag_hash VARCHAR(255) NOT NULL,
                               submitted_flag_masked VARCHAR(255) NOT NULL,
                               is_correct BOOLEAN NOT NULL,
                               create_date_utc TIMESTAMPTZ NOT NULL
                           );

                           CREATE INDEX IF NOT EXISTS ix_laboratory_flag_attempts_student_id_laboratory_work_id ON laboratory_flag_attempts (student_id, laboratory_work_id);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_flag_attempts_is_correct ON laboratory_flag_attempts (is_correct);
                           CREATE INDEX IF NOT EXISTS ix_laboratory_flag_attempts_create_date_utc ON laboratory_flag_attempts (create_date_utc);

                           CREATE TABLE IF NOT EXISTS student_gradebook_records (
                               id UUID PRIMARY KEY,
                               student_id UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
                               group_id UUID NULL REFERENCES groups (id) ON DELETE SET NULL,
                               attendance_percent NUMERIC(5,2) NOT NULL,
                               is_exam_allowed BOOLEAN NOT NULL,
                               has_automatic_grade BOOLEAN NOT NULL,
                               update_date_utc TIMESTAMPTZ NOT NULL,
                               updated_by_teacher_id UUID NULL REFERENCES users (id) ON DELETE SET NULL
                           );

                           CREATE UNIQUE INDEX IF NOT EXISTS ux_student_gradebook_records_student_id ON student_gradebook_records (student_id);
                           CREATE INDEX IF NOT EXISTS ix_student_gradebook_records_group_id ON student_gradebook_records (group_id);
                           CREATE INDEX IF NOT EXISTS ix_student_gradebook_records_is_exam_allowed ON student_gradebook_records (is_exam_allowed);
                           CREATE INDEX IF NOT EXISTS ix_student_gradebook_records_has_automatic_grade ON student_gradebook_records (has_automatic_grade);
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                           DROP TABLE IF EXISTS student_gradebook_records;
                           DROP TABLE IF EXISTS laboratory_flag_attempts;
                           DROP TABLE IF EXISTS laboratory_report_versions;
                           DROP TABLE IF EXISTS laboratory_reports;
                           DROP TABLE IF EXISTS student_laboratory_hints;
                           DROP TABLE IF EXISTS laboratory_hints;
                           DROP TABLE IF EXISTS laboratory_works;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}
