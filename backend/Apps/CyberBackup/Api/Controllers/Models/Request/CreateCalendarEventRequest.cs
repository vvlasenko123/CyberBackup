using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос создания события календаря
/// </summary>
public sealed record CreateCalendarEventRequest
{
    /// <summary>
    /// Название события
    /// </summary>
    [Required(ErrorMessage = "Название события не должно быть пустым")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Дата и время начала события
    /// </summary>
    [Required(ErrorMessage = "Дата начала события не должна быть пустой")]
    public DateTimeOffset StartsAtUtc { get; init; }

    /// <summary>
    /// Дата и время окончания события
    /// </summary>
    [Required(ErrorMessage = "Дата окончания события не должна быть пустой")]
    public DateTimeOffset EndsAtUtc { get; init; }

    /// <summary>
    /// Дата и время уведомления
    /// </summary>
    public DateTimeOffset? NotifyAtUtc { get; init; }
}