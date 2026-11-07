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
    /// Передать диапазон файла отчета непосредственно в целевой поток
    /// </summary>
    Task CopyToAsync(string storagePath, Stream destination, long offset, long length, CancellationToken cancellationToken);
}
