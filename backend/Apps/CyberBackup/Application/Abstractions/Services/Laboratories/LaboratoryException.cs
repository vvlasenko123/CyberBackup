namespace Application.Abstractions.Services.Laboratories;

/// <summary>
/// Бизнес-ошибка лабораторных работ
/// </summary>
public sealed class LaboratoryException : Exception
{
    public LaboratoryException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>
    /// Код ошибки
    /// </summary>
    public string Code { get; }
}
