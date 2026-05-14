namespace Domain.Calendar.Enums;

/// <summary>
/// Статус события календаря
/// </summary>
public enum CalendarEventStatus
{
    /// <summary>
    /// Активное событие
    /// </summary>
    Active = 0,

    /// <summary>
    /// Завершенное событие
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Отмененное событие
    /// </summary>
    Cancelled = 2
}