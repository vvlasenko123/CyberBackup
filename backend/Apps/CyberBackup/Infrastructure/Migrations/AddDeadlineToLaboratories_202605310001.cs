using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

internal sealed class AddDeadlineToLaboratories_202605310001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public AddDeadlineToLaboratories_202605310001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    public string Id => nameof(AddDeadlineToLaboratories_202605310001);

    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
            ALTER TABLE laboratory_works
            ADD COLUMN IF NOT EXISTS deadline_at_utc TIMESTAMPTZ NULL;
            """;
        await _connection.ExecuteAsync(sql, null, token);
    }

    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
            ALTER TABLE laboratory_works
            DROP COLUMN IF EXISTS deadline_at_utc;
            """;
        await _connection.ExecuteAsync(sql, null, token);
    }
}
