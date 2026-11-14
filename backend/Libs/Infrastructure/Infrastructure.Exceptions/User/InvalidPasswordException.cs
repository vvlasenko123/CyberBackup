using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении Password
/// </summary>
public sealed class InvalidPasswordException : BaseException
{
    public InvalidPasswordException(string message) : base(message, ErrorCodes.PasswordInvalid)
    {
    }
}