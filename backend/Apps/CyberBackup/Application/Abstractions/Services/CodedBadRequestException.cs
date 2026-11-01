namespace Application.Abstractions.Services;

/// <summary>
/// Ошибка запроса с машинным кодом. Глобальный обработчик отвечает на неё 400 с телом { code, message }
/// </summary>
public abstract class CodedBadRequestException : Exception
{
    protected CodedBadRequestException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Машинный код ошибки
    /// </summary>
    public string Code { get; }
}
