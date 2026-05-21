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
    /// Отправлен на проверку
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Взят преподавателем на проверку
    /// </summary>
    UnderReview = 2,

    /// <summary>
    /// Требует доработки
    /// </summary>
    RevisionRequired = 3,

    /// <summary>
    /// Принят
    /// </summary>
    Accepted = 4
}
