using Application.Abstractions.Services.Laboratories;
using Application.DTO.Laboratories;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Laboratories;

/// <inheritdoc />
public sealed class LaboratoryReportUploadRequestFactory : ILaboratoryReportUploadRequestFactory
{
    /// <inheritdoc />
    public UploadLaboratoryReportFileDto Create(IFormFile? file)
    {
        if (file is null)
        {
            throw new LaboratoryException("laboratory_report.file_required", "Файл отчета обязателен");
        }

        var stream = file.OpenReadStream();
        var result = new UploadLaboratoryReportFileDto(
            Content: stream,
            FileName: file.FileName,
            ContentType: file.ContentType,
            Length: file.Length);

        return result;
    }
}
