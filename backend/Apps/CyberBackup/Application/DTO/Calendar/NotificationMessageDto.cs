namespace Application.DTO.Calendar;

/// <summary>
/// DTO уведомления для отправки на фронт
/// </summary>
public sealed record NotificationMessageDto(Guid Id, string Title, string Message, DateTimeOffset CreatedAtUtc);