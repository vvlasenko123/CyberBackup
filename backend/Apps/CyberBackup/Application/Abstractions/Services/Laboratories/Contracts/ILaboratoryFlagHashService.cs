namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Сервис хэширования флагов лабораторных работ
/// </summary>
public interface ILaboratoryFlagHashService
{
    string HashFlag(string flag);

    bool VerifyFlag(string flag, string expectedHash);

    string MaskFlag(string flag);
}
