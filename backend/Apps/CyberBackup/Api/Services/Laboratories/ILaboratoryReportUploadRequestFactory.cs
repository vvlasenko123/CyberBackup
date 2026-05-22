using Application.DTO.Laboratories;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Laboratories;

/// <summary>
/// Фабрика файла отчета по лабораторной работе
/// </summary>
public interface ILaboratoryReportUploadRequestFactory
{
    /// <summary>
    /// Создать файл отчета из multipart-запроса
    /// </summary>
    UploadLaboratoryReportFileDto Create(IFormFile? file);
}
