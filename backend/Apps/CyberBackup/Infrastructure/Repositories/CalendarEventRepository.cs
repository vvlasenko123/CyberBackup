using Domain.Calendar;
using Domain.Calendar.Enums;
using Domain.Repositories;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Repositories;

/// <summary>
/// Репозиторий событий календаря
/// </summary>
public sealed class CalendarEventRepository : ICalendarEventRepository
{
    private readonly IAsyncDbConnection _connection;

    public CalendarEventRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task CreateAsync(CalendarEventModel calendarEvent, CancellationToken cancellationToken)
    {
        const string sql = """
                               INSERT INTO calendar_events (
                                   id,
                                   user_id,
                                   title,
                                   event_type,
                                   status,
                                   starts_at_utc,
                                   ends_at_utc,
                                   notify_at_utc,
                                   notified_at_utc,
                                   created_at_utc,
                                   updated_at_utc
                               )
                               VALUES (
                                   @Id,
                                   @UserId,
                                   @Title,
                                   @EventType,
                                   @Status,
                                   @StartsAtUtc,
                                   @EndsAtUtc,
                                   @NotifyAtUtc,
                                   @NotifiedAtUtc,
                                   @CreatedAtUtc,
                                   @UpdatedAtUtc
                               )
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            calendarEvent.Id,
            calendarEvent.UserId,
            calendarEvent.Title,
            EventType = (int)calendarEvent.EventType,
            Status = (int)calendarEvent.Status,
            calendarEvent.StartsAtUtc,
            calendarEvent.EndsAtUtc,
            calendarEvent.NotifyAtUtc,
            calendarEvent.NotifiedAtUtc,
            calendarEvent.CreatedAtUtc,
            calendarEvent.UpdatedAtUtc
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> GetByUserIdAsync(Guid userId,
        CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id AS Id,
                                   user_id AS UserId,
                                   title AS Title,
                                   event_type AS EventType,
                                   status AS Status,
                                   starts_at_utc AS StartsAtUtc,
                                   ends_at_utc AS EndsAtUtc,
                                   notify_at_utc AS NotifyAtUtc,
                                   notified_at_utc AS NotifiedAtUtc,
                                   created_at_utc AS CreatedAtUtc,
                                   updated_at_utc AS UpdatedAtUtc
                               FROM calendar_events
                               WHERE user_id = @UserId
                               ORDER BY starts_at_utc;
                           """;

        var events = await _connection.QueryAsync<CalendarEventModel>(
            sql,
            new
            {
                UserId = userId
            },
            cancellationToken);

        return events.ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id AS Id,
                                   user_id AS UserId,
                                   title AS Title,
                                   event_type AS EventType,
                                   status AS Status,
                                   starts_at_utc AS StartsAtUtc,
                                   ends_at_utc AS EndsAtUtc,
                                   notify_at_utc AS NotifyAtUtc,
                                   notified_at_utc AS NotifiedAtUtc,
                                   created_at_utc AS CreatedAtUtc,
                                   updated_at_utc AS UpdatedAtUtc
                               FROM calendar_events
                               ORDER BY starts_at_utc;
                           """;

        var events = await _connection.QueryAsync<CalendarEventModel>(sql, null, cancellationToken);
        return events.ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> GetForNotificationAsync(
        DateTimeOffset nowUtc,
        int count,
        CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id AS Id,
                                   user_id AS UserId,
                                   title AS Title,
                                   event_type AS EventType,
                                   status AS Status,
                                   starts_at_utc AS StartsAtUtc,
                                   ends_at_utc AS EndsAtUtc,
                                   notify_at_utc AS NotifyAtUtc,
                                   notified_at_utc AS NotifiedAtUtc,
                                   created_at_utc AS CreatedAtUtc,
                                   updated_at_utc AS UpdatedAtUtc
                               FROM calendar_events
                               WHERE notify_at_utc IS NOT NULL
                                 AND notify_at_utc <= @NowUtc
                                 AND notified_at_utc IS NULL
                                 AND status = @Status
                               ORDER BY notify_at_utc
                               LIMIT @Count;
                           """;

        var calendarEventList = await _connection.QueryAsync<CalendarEventModel>(
            sql,
            new
            {
                NowUtc = nowUtc,
                Status = (int)CalendarEventStatus.Active,
                Count = count
            },
            cancellationToken);

        return calendarEventList.ToList();
    }

    /// <inheritdoc />
    public Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM calendar_events
            WHERE id = @Id AND user_id = @UserId
            """;

        return _connection.ExecuteAsync(sql, new { Id = id, UserId = userId }, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM calendar_events
            WHERE id = @Id
            """;

        return _connection.ExecuteAsync(sql, new { Id = id }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<CalendarEventModel>> GetStudentEventsAsync(Guid studentId, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   ce.id AS Id,
                                   ce.user_id AS UserId,
                                   ce.title AS Title,
                                   ce.event_type AS EventType,
                                   ce.status AS Status,
                                   ce.starts_at_utc AS StartsAtUtc,
                                   ce.ends_at_utc AS EndsAtUtc,
                                   ce.notify_at_utc AS NotifyAtUtc,
                                   ce.notified_at_utc AS NotifiedAtUtc,
                                   ce.created_at_utc AS CreatedAtUtc,
                                   ce.updated_at_utc AS UpdatedAtUtc
                               FROM calendar_events ce
                               JOIN users u ON u.id = ce.user_id
                               WHERE u.role >= 2
                                  OR ce.user_id = @StudentId
                                  OR (
                                      u.role = 1
                                      AND ce.user_id IN (
                                          SELECT DISTINCT tg.teacher_id
                                          FROM user_groups ug
                                          JOIN teacher_groups tg ON tg.group_id = ug.group_id
                                          WHERE ug.user_id = @StudentId
                                      )
                                  )
                               ORDER BY ce.starts_at_utc;
                           """;

        var events = await _connection.QueryAsync<CalendarEventModel>(sql, new { StudentId = studentId }, cancellationToken);
        return events.ToList();
    }

    /// <inheritdoc />
    public async Task SetNotifiedAsync(
        Guid id,
        DateTimeOffset notifiedAtUtc,
        CancellationToken cancellationToken)
    {
        const string sql = """
                               UPDATE calendar_events
                               SET
                                   notified_at_utc = @NotifiedAtUtc,
                                   updated_at_utc = @UpdatedAtUtc
                               WHERE id = @Id
                                 AND notified_at_utc IS NULL
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            Id = id,
            NotifiedAtUtc = notifiedAtUtc,
            UpdatedAtUtc = notifiedAtUtc
        }, cancellationToken);
    }
}