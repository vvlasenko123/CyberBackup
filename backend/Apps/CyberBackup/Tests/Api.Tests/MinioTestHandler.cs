using System.Net;
using System.Net.Http.Headers;
using Infrastucture.S3.Options;
using Infrastucture.S3.Storage;
using Microsoft.Extensions.Options;
using Minio;

namespace Api.Tests;

/// <summary>
/// Транспорт для настоящего MinIO SDK: учитывает запросы и прочитанные байты объекта.
/// </summary>
internal sealed class MinioTestHandler(byte[] content) : HttpMessageHandler
{
    public long BytesRead { get; private set; }
    public List<string?> ObjectRanges { get; } = [];

    public MinioObjectStorage CreateStorage(HttpClient httpClient)
    {
        var client = new MinioClient()
            .WithEndpoint("localhost:9000")
            .WithCredentials("test-access", "test-secret")
            .WithRegion("us-east-1")
            .WithSSL(false)
            .WithHttpClient(httpClient)
            .Build();
        return new MinioObjectStorage(client, Options.Create(new MinioOptions { BucketName = "files" }));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = request };
        if (request.Method == HttpMethod.Head)
        {
            response.Content = new ByteArrayContent([]);
            response.Content.Headers.ContentLength = content.Length;
        }
        else
        {
            AssertObjectGet(request);
            ObjectRanges.Add(request.Headers.Range?.ToString());
            var range = request.Headers.Range?.Ranges.Single();
            var from = range?.From ?? 0;
            var to = range?.To ?? content.LongLength - 1;
            var length = to - from + 1;
            response.Content = new StreamContent(new CountingStream(
                new MemoryStream(content, (int)from, (int)length, writable: false), count => BytesRead += count));
            response.Content.Headers.ContentLength = length;
            if (range is not null)
            {
                response.StatusCode = HttpStatusCode.PartialContent;
                response.Content.Headers.ContentRange = new ContentRangeHeaderValue(from, to, content.LongLength);
            }
        }

        response.Headers.ETag = new EntityTagHeaderValue("\"test-object\"");
        response.Content.Headers.LastModified = DateTimeOffset.UtcNow;
        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        return Task.FromResult(response);
    }

    private static void AssertObjectGet(HttpRequestMessage request)
    {
        if (request.Method != HttpMethod.Get || !request.RequestUri!.AbsolutePath.StartsWith("/files/", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unexpected storage request: {request.Method} {request.RequestUri?.AbsolutePath}");
    }

    private sealed class CountingStream(Stream inner, Action<int> onRead) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => inner.Length;
        public override long Position { get => inner.Position; set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = inner.Read(buffer, offset, count);
            onRead(read);
            return read;
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var read = await inner.ReadAsync(buffer, cancellationToken);
            onRead(read);
            return read;
        }
        public override void Flush() => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing)
        {
            if (disposing) inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
