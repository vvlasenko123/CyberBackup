namespace Application.DTO.Calendar;

/// <summary>
/// DTO уведомления из базы данных
/// </summary>
public sealed record GetNotificationDto(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    DateTimeOffset CreatedAtUtc);
