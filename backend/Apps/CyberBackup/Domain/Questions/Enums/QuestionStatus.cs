namespace Domain.Questions.Enums;

/// <summary>
/// Статус вопроса
/// </summary>
public enum QuestionStatus
{
    /// <summary>Открыт — ждёт ответа</summary>
    Open = 0,

    /// <summary>Отвечен — преподаватель ответил</summary>
    Answered = 1,

    /// <summary>Закрыт</summary>
    Closed = 2
}
