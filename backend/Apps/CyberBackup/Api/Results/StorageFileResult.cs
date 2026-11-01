using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Api.Results;

/// <summary>
/// Передаёт нормализованный HTTP-диапазон в хранилище без промежуточной загрузки файла.
/// </summary>
internal sealed class StorageFileResult(
    string contentType,
    long fileLength,
    Func<Stream, long, long, CancellationToken, Task> copyTo) : FileResult(contentType)
{
    public override Task ExecuteResultAsync(ActionContext context)
    {
        var loggerFactory = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        return new Executor(loggerFactory).ExecuteAsync(context, this);
    }

    private sealed class Executor(ILoggerFactory loggerFactory) : FileResultExecutorBase(CreateLogger<Executor>(loggerFactory))
    {
        public async Task ExecuteAsync(ActionContext context, StorageFileResult result)
        {
            var (range, rangeLength, serveBody) = SetHeadersAndLog(
                context, result, result.FileLength, result.EnableRangeProcessing, result.LastModified, result.EntityTag);

            if (!serveBody || result.FileLength == 0)
                return;

            var response = context.HttpContext.Response;
            var offset = range?.From ?? 0;
            var length = range is null ? result.FileLength : rangeLength;
            try
            {
                await result.CopyTo(response.Body, offset, length, context.HttpContext.RequestAborted);
            }
            catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
            {
                context.HttpContext.Abort();
            }
        }
    }

    private long FileLength { get; } = fileLength;
    private Func<Stream, long, long, CancellationToken, Task> CopyTo { get; } = copyTo;
}
