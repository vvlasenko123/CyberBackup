namespace Domain.Laboratories.Enums;

/// <summary>
/// Статус отчета по лабораторной работе
/// </summary>
public enum LaboratoryReportStatus
{
    /// <summary>
    /// Не отправлен
    /// </summary>
    NotSubmitted = 0,

    /// <summary>
    /// Ожидает проверки
    /// </summary>
    PendingReview = 1,

    /// <summary>
    /// Принят
    /// </summary>
    Accepted = 2,

    /// <summary>
    /// Отклонен
    /// </summary>
    Rejected = 3
}
