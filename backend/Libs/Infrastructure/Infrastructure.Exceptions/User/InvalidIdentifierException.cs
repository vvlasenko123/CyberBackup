using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении Id
/// </summary>
public sealed class InvalidIdentifierException : BaseException
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    private const string ErrorCode = "identifier.invalid";

    public InvalidIdentifierException(string message) : base(message, ErrorCode)
    {
    }
}