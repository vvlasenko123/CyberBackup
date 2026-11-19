using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

internal sealed class AddLaboratoryReadIndexes_202611140001(IAsyncDbConnection connection) : IDatabaseMigration
{
    public string Id => nameof(AddLaboratoryReadIndexes_202611140001);

    public Task MigrateUp(CancellationToken token)
        => connection.ExecuteAsync("""
            CREATE INDEX IF NOT EXISTS ix_user_groups_user_id_group_id
                ON user_groups (user_id, group_id);
            CREATE INDEX IF NOT EXISTS ix_laboratory_flag_attempts_correct_student_work
                ON laboratory_flag_attempts (student_id, laboratory_work_id)
                WHERE is_correct;
            """, null, token);

    public Task MigrateDown(CancellationToken token)
        => connection.ExecuteAsync("""
            DROP INDEX IF EXISTS ix_laboratory_flag_attempts_correct_student_work;
            DROP INDEX IF EXISTS ix_user_groups_user_id_group_id;
            """, null, token);
}
