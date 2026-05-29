using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция создания таблиц вопросов студентов
/// </summary>
internal sealed class CreateQuestions_202605270002 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public CreateQuestions_202605270002(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    public string Id => nameof(CreateQuestions_202605270002);

    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                           CREATE TABLE IF NOT EXISTS questions (
                               id UUID PRIMARY KEY,
                               student_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                               laboratory_title VARCHAR(255) NULL,
                               description VARCHAR(3000) NOT NULL,
                               status INT NOT NULL DEFAULT 0,
                               created_at_utc TIMESTAMPTZ NOT NULL
                           );

                           CREATE INDEX IF NOT EXISTS ix_questions_student_id ON questions(student_id);
                           CREATE INDEX IF NOT EXISTS ix_questions_status ON questions(status);
                           CREATE INDEX IF NOT EXISTS ix_questions_created_at_utc ON questions(created_at_utc DESC);

                           CREATE TABLE IF NOT EXISTS question_replies (
                               id UUID PRIMARY KEY,
                               question_id UUID NOT NULL REFERENCES questions(id) ON DELETE CASCADE,
                               teacher_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                               content VARCHAR(3000) NOT NULL,
                               created_at_utc TIMESTAMPTZ NOT NULL,
                               UNIQUE(question_id)
                           );
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                           DROP TABLE IF EXISTS question_replies;
                           DROP TABLE IF EXISTS questions;
                           """;
        await _connection.ExecuteAsync(sql, null, token);
    }
}
