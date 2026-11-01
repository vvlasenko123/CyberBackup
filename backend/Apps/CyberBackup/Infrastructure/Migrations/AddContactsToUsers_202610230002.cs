using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Добавляет в пользователей контакты: телефон, Telegram и VK
/// </summary>
internal sealed class AddContactsToUsers_202610230002(IAsyncDbConnection connection) : IDatabaseMigration
{
    public string Id => nameof(AddContactsToUsers_202610230002);

    public Task MigrateUp(CancellationToken token) => connection.ExecuteAsync("""
        ALTER TABLE users ADD COLUMN IF NOT EXISTS phone VARCHAR(32) NULL;
        ALTER TABLE users ADD COLUMN IF NOT EXISTS telegram VARCHAR(64) NULL;
        ALTER TABLE users ADD COLUMN IF NOT EXISTS vk VARCHAR(64) NULL;
        """, null, token);

    public Task MigrateDown(CancellationToken token) => connection.ExecuteAsync("""
        ALTER TABLE users DROP COLUMN IF EXISTS vk;
        ALTER TABLE users DROP COLUMN IF EXISTS telegram;
        ALTER TABLE users DROP COLUMN IF EXISTS phone;
        """, null, token);
}
