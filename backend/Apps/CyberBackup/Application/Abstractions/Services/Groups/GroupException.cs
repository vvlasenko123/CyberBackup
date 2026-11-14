using Infrastructure.Exceptions.Base;

namespace Application.Abstractions.Services.Groups;

/// <summary>Бизнес-ошибка управления группами.</summary>
public sealed class GroupException(string code, string message) : BaseException(message, code);
