namespace Application.DTO.Laboratories;

/// <summary>
/// Файл отчета для загрузки
/// </summary>
public sealed record UploadLaboratoryReportFileDto(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);
