namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Сервис хэширования флагов лабораторных работ
/// </summary>
public interface ILaboratoryFlagHashService
{
    /// <summary>
    /// Получить хэш флага
    /// </summary>
    string HashFlag(string flag);

    /// <summary>
    /// Проверить флаг по ожидаемому хэшу
    /// </summary>
    bool VerifyFlag(string flag, string expectedHash);

    /// <summary>
    /// Замаскировать флаг для хранения попытки
    /// </summary>
    string MaskFlag(string flag);
}
