using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

internal sealed class AddQuestionAttachments_202610140001(IAsyncDbConnection connection) : IDatabaseMigration
{
    public string Id => nameof(AddQuestionAttachments_202610140001);
    public Task MigrateUp(CancellationToken token) => connection.ExecuteAsync("""
        CREATE TABLE IF NOT EXISTS question_attachments (
            id UUID PRIMARY KEY,
            reply_id UUID NOT NULL REFERENCES question_replies(id) ON DELETE CASCADE,
            file_name TEXT NOT NULL,
            content_type TEXT NOT NULL,
            size_bytes BIGINT NOT NULL,
            storage_path TEXT NOT NULL
        );
        CREATE INDEX IF NOT EXISTS ix_question_attachments_reply ON question_attachments(reply_id);
        """, null, token);
    public Task MigrateDown(CancellationToken token) => connection.ExecuteAsync(
        "DROP TABLE IF EXISTS question_attachments;", null, token);
}
