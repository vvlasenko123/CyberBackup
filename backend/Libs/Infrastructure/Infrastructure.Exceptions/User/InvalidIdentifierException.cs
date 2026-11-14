using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении Id
/// </summary>
public sealed class InvalidIdentifierException : BaseException
{
    public InvalidIdentifierException(string message) : base(message, ErrorCodes.IdentifierInvalid)
    {
    }
}