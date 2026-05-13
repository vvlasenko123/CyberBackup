namespace Domain.Laboratories.Enums;

/// <summary>
/// Статус лабораторной работы для студента
/// </summary>
public enum StudentLaboratoryStatus
{
    /// <summary>
    /// Не начата
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// В работе
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Ожидает проверки
    /// </summary>
    PendingReview = 2,

    /// <summary>
    /// Принята
    /// </summary>
    Accepted = 3,

    /// <summary>
    /// Отклонена
    /// </summary>
    Rejected = 4
}
