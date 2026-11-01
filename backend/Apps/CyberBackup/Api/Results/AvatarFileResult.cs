using Application.DTO.Avatars;
using Microsoft.AspNetCore.Mvc;

namespace Api.Results;

/// <summary>
/// Ответ на чтение аватара: 304 или файл, с заголовками кэширования и защиты от сниффинга
/// </summary>
public sealed class AvatarFileResult : IActionResult
{
    private readonly AvatarDownload _avatar;

    public AvatarFileResult(AvatarDownload avatar)
    {
        _avatar = avatar;
    }

    /// <inheritdoc />
    public Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;

        response.Headers.CacheControl = "private, no-cache";
        response.Headers.ETag = _avatar.ETag;
        response.Headers["X-Content-Type-Options"] = "nosniff";

        if (_avatar.NotModified)
        {
            response.StatusCode = StatusCodes.Status304NotModified;

            return Task.CompletedTask;
        }

        response.Headers.ContentDisposition = "inline";

        return new FileStreamResult(_avatar.Content!, _avatar.ContentType!).ExecuteResultAsync(context);
    }
}
