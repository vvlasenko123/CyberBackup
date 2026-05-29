namespace Application.Abstractions.Services.Groups;

/// <summary>
/// Бизнес-ошибка управления группами
/// </summary>
public sealed class GroupException : Exception
{
    public GroupException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
