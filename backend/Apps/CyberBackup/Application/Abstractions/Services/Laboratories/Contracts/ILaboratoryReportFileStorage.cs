using Application.DTO.Laboratories;

namespace Application.Abstractions.Services.Laboratories.Contracts;

/// <summary>
/// Хранилище файлов отчетов по лабораторным работам
/// </summary>
public interface ILaboratoryReportFileStorage
{
    Task<SavedLaboratoryReportFileDto> SaveAsync(
        UploadLaboratoryReportFileDto file,
        CancellationToken cancellationToken);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken);
}
