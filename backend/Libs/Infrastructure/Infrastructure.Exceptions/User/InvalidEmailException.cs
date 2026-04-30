using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении Email
/// </summary>
public sealed class InvalidEmailException : BaseException
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    private const string ErrorCode = "email.invalid";

    /// <summary>
    /// Создает исключение с сообщением и кодом ошибки
    /// </summary>
    public InvalidEmailException(string message)
        : base(message, ErrorCode)
    {
    }
}