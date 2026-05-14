using Application.DTO.Laboratories;

namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Хранилище файлов отчетов по лабораторным работам
/// </summary>
public interface ILaboratoryReportFileStorage
{
    /// <summary>
    /// Сохранить файл отчета
    /// </summary>
    Task<SavedLaboratoryReportFileDto> SaveAsync(
        UploadLaboratoryReportFileDto file,
        CancellationToken cancellationToken);

    /// <summary>
    /// Открыть файл отчета для чтения
    /// </summary>
    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}
