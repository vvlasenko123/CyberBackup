using Application.Abstractions.Services.Laboratories.Contracts;
using Application.DTO.Laboratories;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class LaboratoryReportFileStorage : ILaboratoryReportFileStorage
{
    private readonly string _rootPath;

    public LaboratoryReportFileStorage()
    {
        _rootPath = Path.Combine(AppContext.BaseDirectory, "laboratory-report-files");
    }

    /// <inheritdoc />
    public async Task<SavedLaboratoryReportFileDto> SaveAsync(
        UploadLaboratoryReportFileDto file,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_rootPath);

        var extension = Path.GetExtension(file.FileName);
        var storedFileName = $"{UUIDNext.Uuid.NewSequential():N}{extension}";
        var path = Path.Combine(_rootPath, storedFileName);

        await using var destination = File.Create(path);
        await file.Content.CopyToAsync(destination, cancellationToken);

        return new SavedLaboratoryReportFileDto(
            StoragePath: path,
            OriginalFileName: file.FileName,
            ContentType: file.ContentType,
            FileSizeBytes: file.Length);
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken)
    {
        Stream stream = File.OpenRead(storagePath);

        return Task.FromResult(stream);
    }
}
