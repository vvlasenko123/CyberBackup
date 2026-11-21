using Api.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Api.Extensions;

/// <summary>
/// Ответы с защищёнными файлами после проверки доступа к ресурсу.
/// </summary>
public static class FileResponseExtensions
{
    private static readonly HashSet<string> InlineContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/gif"
    };

    /// <summary>
    /// Отдать файл без кэширования; поддерживаемые форматы можно открыть в браузере.
    /// Проверка прав выполняется до создания результата; чтение хранилища — при передаче тела.
    /// </summary>
    public static FileResult ProtectedFile(
        this ControllerBase controller,
        string contentType,
        string fileName,
        long fileLength,
        bool inline,
        Func<Stream, long, long, CancellationToken, Task> copyTo)
    {
        var preview = inline && InlineContentTypes.Contains(contentType);

        controller.Response.Headers.CacheControl = "private, no-store";
        controller.Response.Headers["X-Content-Type-Options"] = "nosniff";

        if (preview)
        {
            var disposition = new ContentDispositionHeaderValue("inline");
            disposition.SetHttpFileName(fileName);
            controller.Response.Headers.ContentDisposition = disposition.ToString();
        }

        return new StorageFileResult(contentType, fileLength, copyTo)
        {
            FileDownloadName = preview ? string.Empty : fileName,
            EnableRangeProcessing = preview
        };
    }
}
