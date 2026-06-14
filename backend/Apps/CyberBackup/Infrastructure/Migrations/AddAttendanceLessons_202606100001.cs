using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

internal sealed class AddAttendanceLessons_202606100001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public AddAttendanceLessons_202606100001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    public string Id => nameof(AddAttendanceLessons_202606100001);

    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
            ALTER TABLE student_gradebook_records
                ADD COLUMN IF NOT EXISTS lessons_attended INTEGER NOT NULL DEFAULT 0,
                ADD COLUMN IF NOT EXISTS total_lessons    INTEGER NOT NULL DEFAULT 0;
            """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
            ALTER TABLE student_gradebook_records
                DROP COLUMN IF EXISTS lessons_attended,
                DROP COLUMN IF EXISTS total_lessons;
            """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}
