using Infrastructure.Exceptions.Base;

namespace Application.Abstractions.Services.Questions;

/// <summary>Бизнес-ошибка вопросов.</summary>
public sealed class QuestionException(string code, string message) : BaseException(message, code);
