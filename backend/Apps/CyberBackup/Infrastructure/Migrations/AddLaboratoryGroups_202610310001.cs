using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

internal sealed class AddLaboratoryGroups_202610310001(IAsyncDbConnection connection) : IDatabaseMigration
{
    public string Id => nameof(AddLaboratoryGroups_202610310001);

    public async Task MigrateUp(CancellationToken token)
    {
        await connection.ExecuteAsync("""
            ALTER TABLE laboratory_works ADD COLUMN restrict_to_groups BOOLEAN NOT NULL DEFAULT false;
            CREATE TABLE laboratory_groups (
                laboratory_work_id UUID NOT NULL REFERENCES laboratory_works(id) ON DELETE CASCADE,
                group_id UUID NOT NULL REFERENCES groups(id) ON DELETE CASCADE,
                PRIMARY KEY (laboratory_work_id, group_id)
            );
            CREATE INDEX ix_laboratory_groups_group_id ON laboratory_groups(group_id);
            """, null, token);
    }

    public async Task MigrateDown(CancellationToken token)
    {
        await connection.ExecuteAsync("""
            DROP TABLE laboratory_groups;
            ALTER TABLE laboratory_works DROP COLUMN restrict_to_groups;
            """, null, token);
    }
}
