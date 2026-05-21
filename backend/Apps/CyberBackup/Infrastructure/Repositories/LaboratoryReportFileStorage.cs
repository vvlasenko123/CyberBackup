using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;
using Infrastucture.S3.Storage;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class LaboratoryReportFileStorage : ILaboratoryReportFileStorage
{
    private const string ReportsPrefix = "laboratory-reports";

    private readonly MinioObjectStorage _storage;

    public LaboratoryReportFileStorage(MinioObjectStorage storage)
    {
        _storage = storage;
    }

    /// <inheritdoc />
    public async Task<SavedLaboratoryReportFileDto> SaveAsync(
        UploadLaboratoryReportFileDto file,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(file.FileName);
        var objectName = $"{ReportsPrefix}/{UUIDNext.Uuid.NewSequential():N}{extension}";

        await _storage.SaveAsync(
            objectName,
            file.Content,
            file.Length,
            file.ContentType,
            cancellationToken);

        return new SavedLaboratoryReportFileDto(
            StoragePath: objectName,
            OriginalFileName: file.FileName,
            ContentType: file.ContentType,
            FileSizeBytes: file.Length);
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        var result = _storage.OpenReadAsync(storagePath, cancellationToken);

        return result;
    }
}
