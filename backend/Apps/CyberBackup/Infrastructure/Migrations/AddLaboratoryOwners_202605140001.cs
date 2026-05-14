using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Миграция добавления владельца лабораторной работы
/// </summary>
internal sealed class AddLaboratoryOwners_202605140001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public AddLaboratoryOwners_202605140001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public string Id => nameof(AddLaboratoryOwners_202605140001);

    /// <inheritdoc />
    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                           ALTER TABLE laboratory_works
                           ADD COLUMN IF NOT EXISTS created_by_teacher_id UUID NULL REFERENCES users (id) ON DELETE SET NULL;

                           CREATE INDEX IF NOT EXISTS ix_laboratory_works_created_by_teacher_id
                           ON laboratory_works (created_by_teacher_id);
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    /// <inheritdoc />
    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                           DROP INDEX IF EXISTS ix_laboratory_works_created_by_teacher_id;

                           ALTER TABLE laboratory_works
                           DROP COLUMN IF EXISTS created_by_teacher_id;
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}
