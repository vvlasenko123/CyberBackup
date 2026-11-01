using Application.DTO.Avatars;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Avatars;

/// <summary>
/// Фабрика файла аватара
/// </summary>
public interface IAvatarUploadRequestFactory
{
    /// <summary>
    /// Создать файл аватара из multipart-запроса, null - файл не передан
    /// </summary>
    AvatarUpload? Create(IFormFile? file);
}
