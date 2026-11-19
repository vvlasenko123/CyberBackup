using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении Email
/// </summary>
public sealed class InvalidEmailException : BaseException
{
    /// <summary>
    /// Создает исключение с сообщением и кодом ошибки
    /// </summary>
    public InvalidEmailException(string message)
        : base(message, ErrorCodes.EmailInvalid)
    {
    }
}