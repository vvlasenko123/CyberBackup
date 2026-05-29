using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Database.Migrations.Contracts;

namespace Infrastructure.Migrations;

/// <summary>
/// Переводим question_replies в чат-режим:
/// убираем UNIQUE(question_id), заменяем teacher_id на sender_id + is_from_teacher
/// </summary>
internal sealed class UpdateQuestionRepliesForChat_202605280001 : IDatabaseMigration
{
    private readonly IAsyncDbConnection _connection;

    public UpdateQuestionRepliesForChat_202605280001(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    public string Id => nameof(UpdateQuestionRepliesForChat_202605280001);

    public async Task MigrateUp(CancellationToken token)
    {
        const string sql = """
                           -- Добавляем sender_id (автор сообщения — студент или преподаватель)
                           ALTER TABLE question_replies
                               ADD COLUMN IF NOT EXISTS sender_id UUID REFERENCES users(id) ON DELETE CASCADE;

                           -- Переносим существующие записи
                           UPDATE question_replies SET sender_id = teacher_id WHERE sender_id IS NULL;

                           ALTER TABLE question_replies ALTER COLUMN sender_id SET NOT NULL;

                           -- Флаг: сообщение от преподавателя (true) или студента (false)
                           ALTER TABLE question_replies
                               ADD COLUMN IF NOT EXISTS is_from_teacher BOOLEAN NOT NULL DEFAULT true;

                           -- Убираем старый столбец teacher_id
                           ALTER TABLE question_replies DROP COLUMN IF EXISTS teacher_id;

                           -- Снимаем ограничение «один ответ на вопрос»
                           ALTER TABLE question_replies
                               DROP CONSTRAINT IF EXISTS question_replies_question_id_key;

                           -- Индекс для быстрой выборки по вопросу в хронологическом порядке
                           CREATE INDEX IF NOT EXISTS ix_question_replies_question_id_ts
                               ON question_replies(question_id, created_at_utc);
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }

    public async Task MigrateDown(CancellationToken token)
    {
        const string sql = """
                           DROP INDEX IF EXISTS ix_question_replies_question_id_ts;

                           ALTER TABLE question_replies
                               ADD COLUMN IF NOT EXISTS teacher_id UUID REFERENCES users(id) ON DELETE CASCADE;
                           UPDATE question_replies SET teacher_id = sender_id;

                           ALTER TABLE question_replies DROP COLUMN IF EXISTS sender_id;
                           ALTER TABLE question_replies DROP COLUMN IF EXISTS is_from_teacher;

                           ALTER TABLE question_replies
                               ADD CONSTRAINT question_replies_question_id_key UNIQUE(question_id);
                           """;

        await _connection.ExecuteAsync(sql, null, token);
    }
}
