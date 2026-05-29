namespace Application.Abstractions.Services.Questions;

/// <summary>
/// Исключение домена вопросов
/// </summary>
public sealed class QuestionException : Exception
{
    public string Code { get; }

    public QuestionException(string code, string message) : base(message)
    {
        Code = code;
    }
}
