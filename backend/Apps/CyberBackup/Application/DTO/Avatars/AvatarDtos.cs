namespace Application.DTO.Avatars;

/// <summary>
/// Ограничения аватара
/// </summary>
public static class AvatarLimits
{
    /// <summary>
    /// Максимальный размер загружаемого файла и перекодированного результата
    /// </summary>
    public const int MaxBytes = 1_048_576;

    public const int MinSide = 64;

    public const int MaxSide = 4096;
}

/// <summary>
/// Загруженный файл аватара
/// </summary>
public sealed record AvatarUpload(Stream Content, string FileName, string ContentType, long Length);

/// <summary>
/// Результат перекодирования аватара
/// </summary>
/// <param name="Bytes">Содержимое перекодированного файла</param>
/// <param name="Extension">Расширение без точки: jpg, png или webp</param>
/// <param name="ContentType">MIME-тип, соответствующий расширению</param>
public sealed record ProcessedAvatar(byte[] Bytes, string Extension, string ContentType);

/// <summary>
/// Результат чтения аватара
/// </summary>
/// <param name="ETag">Непрозрачный ETag в кавычках</param>
/// <param name="NotModified">Клиентская копия актуальна, содержимое не читалось</param>
/// <param name="ContentType">MIME-тип, null при NotModified</param>
/// <param name="Content">Содержимое, null при NotModified</param>
public sealed record AvatarDownload(string ETag, bool NotModified, string? ContentType, Stream? Content);
