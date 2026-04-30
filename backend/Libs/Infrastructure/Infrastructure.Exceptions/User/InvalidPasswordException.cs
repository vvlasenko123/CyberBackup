using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении Password
/// </summary>
public sealed class InvalidPasswordException : BaseException
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    private const string ErrorCode = "password.invalid";

    public InvalidPasswordException(string message) : base(message, ErrorCode)
    {
    }
}