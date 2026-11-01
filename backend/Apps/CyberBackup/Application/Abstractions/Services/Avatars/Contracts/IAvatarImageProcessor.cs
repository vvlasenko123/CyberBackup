using Application.DTO.Avatars;

namespace Application.Abstractions.Services.Avatars.Contracts;

/// <summary>
/// Проверка и перекодирование загруженного изображения
/// </summary>
public interface IAvatarImageProcessor
{
    /// <summary>
    /// Проверить изображение и перекодировать его без исходных метаданных.
    /// Нарушения требований выбрасываются как <see cref="AvatarException"/>
    /// </summary>
    /// <param name="content">Содержимое файла, не больше <see cref="AvatarLimits.MaxBytes"/> байт</param>
    /// <param name="fileName">Имя загруженного файла (используется только расширение)</param>
    /// <param name="contentType">Content-Type загруженного файла</param>
    ProcessedAvatar Process(byte[] content, string fileName, string contentType);
}
