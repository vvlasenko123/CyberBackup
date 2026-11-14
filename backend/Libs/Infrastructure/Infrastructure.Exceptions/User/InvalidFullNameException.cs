using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении FullName
/// </summary>
public sealed class InvalidFullNameException : BaseException
{
    public InvalidFullNameException(string message) : base(message, ErrorCodes.FullNameInvalid)
    {
    }
}