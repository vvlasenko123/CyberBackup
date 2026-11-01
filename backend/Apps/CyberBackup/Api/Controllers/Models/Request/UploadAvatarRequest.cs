using Microsoft.AspNetCore.Http;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос загрузки аватара
/// </summary>
public sealed record UploadAvatarRequest
{
    /// <summary>
    /// Изображение в формате JPEG, PNG или WebP
    /// </summary>
    public IFormFile? File { get; init; }
}
