namespace Domain.Calendar.Enums;

/// <summary>
/// Тип события календаря
/// </summary>
public enum CalendarEventType
{
    /// <summary>
    /// Лекция
    /// </summary>
    Lecture = 0,

    /// <summary>
    /// Вебинар
    /// </summary>
    Webinar = 1,

    /// <summary>
    /// Экзамен
    /// </summary>
    Exam = 2,

    /// <summary>
    /// Лабораторная работа
    /// </summary>
    Lab = 3,

    /// <summary>
    /// Другое
    /// </summary>
    Other = 4
}
