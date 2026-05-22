using Microsoft.AspNetCore.Http;

namespace Api.Controllers.Models.Request.Laboratories;

/// <summary>
/// Запрос загрузки отчета по лабораторной работе
/// </summary>
public sealed record UploadLaboratoryReportRequest
{
    /// <summary>
    /// Файл отчета
    /// </summary>
    public IFormFile? File { get; init; }
}
