using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Добавляет в пользователей ключ аватара в объектном хранилище и время его загрузки
/// </summary>
internal sealed class AddAvatarToUsers_202610230001(IAsyncDbConnection connection) : IDatabaseMigration
{
    public string Id => nameof(AddAvatarToUsers_202610230001);

    public Task MigrateUp(CancellationToken token) => connection.ExecuteAsync("""
        ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar_path VARCHAR(512) NULL;
        ALTER TABLE users ADD COLUMN IF NOT EXISTS avatar_updated_at TIMESTAMPTZ NULL;

        DO $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1
                FROM pg_constraint
                WHERE conname = 'ck_users_avatar_consistent'
                  AND conrelid = 'users'::regclass
            ) THEN
                ALTER TABLE users
                ADD CONSTRAINT ck_users_avatar_consistent
                CHECK ((avatar_path IS NULL) = (avatar_updated_at IS NULL));
            END IF;
        END
        $$;
        """, null, token);

    public Task MigrateDown(CancellationToken token) => connection.ExecuteAsync("""
        ALTER TABLE users DROP CONSTRAINT IF EXISTS ck_users_avatar_consistent;
        ALTER TABLE users DROP COLUMN IF EXISTS avatar_updated_at;
        ALTER TABLE users DROP COLUMN IF EXISTS avatar_path;
        """, null, token);
}
