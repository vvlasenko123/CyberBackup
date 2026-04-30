using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions.User;

/// <summary>
/// Исключение, выбрасываемое при некорректном значении FullName
/// </summary>
public sealed class InvalidFullNameException : BaseException
{
    /// <summary>
    /// Код ошибки
    /// </summary>
    private const string ErrorCode = "full_name.invalid";

    public InvalidFullNameException(string message) : base(message, ErrorCode)
    {
    }
}