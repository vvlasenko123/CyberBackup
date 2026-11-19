using Infrastructure.Exceptions.Base;

namespace Infrastructure.Exceptions;

/// <summary>Ожидаемая ошибка прикладной операции.</summary>
public sealed class BusinessException(string code, string message) : BaseException(message, code);
