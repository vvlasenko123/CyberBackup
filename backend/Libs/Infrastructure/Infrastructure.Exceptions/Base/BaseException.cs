namespace Infrastructure.Exceptions.Base;

/// <summary>
/// Базовое исключение приложения с кодом ошибки
/// </summary>
public abstract class BaseException : Exception
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    protected string Code { get; }

    /// <summary>
    /// Создает исключение с сообщением и кодом ошибки
    /// </summary>
    protected BaseException(string message, string code) : base(message)
    {
        Code = code;
    }
}