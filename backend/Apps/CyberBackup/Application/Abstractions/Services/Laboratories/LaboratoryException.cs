using Infrastructure.Exceptions.Base;

namespace Application.Abstractions.Services.Laboratories;

/// <summary>Бизнес-ошибка лабораторных работ.</summary>
public sealed class LaboratoryException(string code, string message) : BaseException(message, code);
