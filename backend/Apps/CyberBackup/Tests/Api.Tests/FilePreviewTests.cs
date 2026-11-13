using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Api;
using Api.Controllers;
using Api.Extensions;
using Api.Services.Laboratories;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.Services.Laboratories;
using Application.Abstractions.Services.Laboratories.Contracts;
using Application.Abstractions.Services.Posts.Contracts;
using Application.Abstractions.Services.Questions;
using Application.Abstractions.Services.Questions.Contracts;
using Application.DTO.Auth;
using Application.DTO.Laboratories;
using Application.DTO.Questions;
using Domain.Questions.Enums;
using Domain.Repositories;
using Infrastructure.Auth.Options;
using Infrastructure.Auth.Services;
using Infrastructure.Core.Controllers;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Security.Host.Cors;
using Security.Host.Cors.Options;
using Xunit;

namespace Api.Tests;

/// <summary>
/// Проверяет HTTP-конвейер, JWT и реальные сервисы без внешних БД и MinIO.
/// Репозитории возвращают файл только для заданных связей ресурса и пользователя.
/// </summary>
public sealed class FilePreviewTests : IDisposable
{
    private static readonly Guid StudentId = Guid.NewGuid();
    private static readonly Guid TeacherId = Guid.NewGuid();
    private static readonly Guid LaboratoryId = Guid.NewGuid();
    private static readonly Guid ReportId = Guid.NewGuid();
    private static readonly Guid VersionId = Guid.NewGuid();
    private static readonly Guid QuestionId = Guid.NewGuid();
    private static readonly Guid AttachmentId = Guid.NewGuid();
    private static readonly byte[] FileBytes = Encoding.UTF8.GetBytes("%PDF-1.4\nexample file");
    private static readonly JwtOptions JwtSettings = new()
    {
        Issuer = "file-preview-tests",
        Audience = "file-preview-tests",
        SigningKey = "file-preview-tests-signing-key-at-least-32-bytes"
    };

    private readonly Mock<ILaboratoryRepository> _laboratories = new();
    private readonly Mock<IQuestionRepository> _questions = new();
    private readonly Mock<ILaboratoryReportFileStorage> _reportStorage = new(MockBehavior.Strict);
    private readonly Mock<IQuestionAttachmentStorage> _attachmentStorage = new(MockBehavior.Strict);
    private readonly IHost _host;
    private readonly HttpClient _client;
    private string _contentType = "application/pdf";
    private string _fileName = "Отчёт.pdf";
    private long _fileSizeBytes = FileBytes.LongLength;

    public FilePreviewTests()
    {
        _laboratories.Setup(x => x.GetStudentReportFileAsync(
                StudentId, LaboratoryId, VersionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ReportFile());
        _laboratories.Setup(x => x.GetReportFileAsync(
                ReportId, VersionId, TeacherId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ReportFile());
        _laboratories.Setup(x => x.GetReportFileAsync(
                ReportId, VersionId, It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ReportFile());

        _questions.Setup(x => x.GetQuestionDetailAsync(QuestionId, StudentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Question());
        _questions.Setup(x => x.GetQuestionDetailForTeacherAsync(
                QuestionId, TeacherId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Question());
        _questions.Setup(x => x.GetQuestionDetailForTeacherAsync(
                QuestionId, It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => Question());

        _reportStorage.Setup(x => x.CopyToAsync("reports/example", It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((string path, Stream destination, long offset, long length, CancellationToken ct) =>
                destination.WriteAsync(FileBytes.AsMemory((int)offset, (int)length), ct).AsTask());
        _attachmentStorage.Setup(x => x.CopyToAsync("attachments/example", It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((string path, Stream destination, long offset, long length, CancellationToken ct) =>
                destination.WriteAsync(FileBytes.AsMemory((int)offset, (int)length), ct).AsTask());

        _host = new HostBuilder().ConfigureWebHost(web => web
            .UseTestServer()
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
            {
                services.AddCyberControllers();
                services.AddControllers().AddApplicationPart(typeof(LaboratoriesController).Assembly);
                services.AddCyberDefaultCors();
                services.AddSingleton<IOptions<CyberCorsOptions>>(
                    Options.Create(new CyberCorsOptions { Origins = ["http://localhost"] }));
                services.AddHealthChecks();
                services.AddSignalR();
                services.AddHttpContextAccessor();
                services.AddSingleton<IOptions<JwtOptions>>(Options.Create(JwtSettings));
                services.AddCyberJwtAuthentication();
                services.AddScoped<IJwtService, JwtService>();
                services.AddSingleton(_laboratories.Object);
                services.AddSingleton(_questions.Object);
                services.AddSingleton(_reportStorage.Object);
                services.AddSingleton(_attachmentStorage.Object);
                services.AddSingleton(Mock.Of<INotificationRepository>());
                services.AddSingleton(Mock.Of<INotificationPushService>());
                services.AddSingleton(Mock.Of<IPostRepository>());
                services.AddSingleton<ILaboratoryFlagHashService, LaboratoryFlagHashService>();
                services.AddScoped<ILaboratoryReportUploadRequestFactory, LaboratoryReportUploadRequestFactory>();
                services.AddScoped<ILaboratoryService, LaboratoryService>();
                services.AddScoped<IQuestionService, QuestionService>();
            })
            .Configure(app => new StartUp(
                    app.ApplicationServices.GetRequiredService<IWebHostEnvironment>(),
                    new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build())
                .Configure(app)))
            .Start();
        _client = _host.GetTestClient();
    }

    [Theory]
    [InlineData("student-report", "student")]
    [InlineData("teacher-report", "teacher")]
    [InlineData("student-attachment", "student")]
    [InlineData("teacher-attachment", "teacher")]
    [InlineData("teacher-report", "admin")]
    [InlineData("teacher-report", "superadmin")]
    [InlineData("teacher-attachment", "admin")]
    [InlineData("teacher-attachment", "superadmin")]
    public async Task Authorized_user_can_preview_file_with_private_headers(string route, string role)
    {
        using var response = await GetFile(route, role, inline: true, cookie: true);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(_fileName, response.Content.Headers.ContentDisposition?.FileNameStar);
        Assert.True(response.Headers.CacheControl?.NoStore);
        Assert.True(response.Headers.CacheControl?.Private);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal(FileBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("student-report", "student")]
    [InlineData("teacher-report", "teacher")]
    [InlineData("student-attachment", "student")]
    [InlineData("teacher-attachment", "teacher")]
    public async Task Default_request_still_downloads_the_file(string route, string role)
    {
        using var response = await GetFile(route, role);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(_fileName, response.Content.Headers.ContentDisposition?.FileNameStar);
        Assert.Equal(FileBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("image/png", "image.png")]
    [InlineData("image/jpeg", "image.jpg")]
    [InlineData("image/gif", "image.gif")]
    public async Task Supported_images_are_returned_inline(string contentType, string fileName)
    {
        _contentType = contentType;
        _fileName = fileName;
        using var response = await GetFile("student-attachment", "student", inline: true);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(contentType, response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
    }

    [Theory]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "report.docx")]
    [InlineData("text/html", "page.html")]
    [InlineData("image/svg+xml", "image.svg")]
    public async Task Other_types_keep_download_disposition_even_if_inline_is_requested(string contentType, string fileName)
    {
        _contentType = contentType;
        _fileName = fileName;
        using var response = await GetFile("student-attachment", "student", inline: true);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(FileBytes, await response.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("student-report")]
    [InlineData("teacher-report")]
    [InlineData("student-attachment")]
    [InlineData("teacher-attachment")]
    public async Task Anonymous_request_is_rejected_without_opening_storage(string route)
    {
        using var response = await _client.GetAsync(PathFor(route) + "?inline=true");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        AssertStorageNotOpened();
        Assert.Empty(_laboratories.Invocations);
        Assert.Empty(_questions.Invocations);
    }

    [Theory]
    [InlineData("teacher-report", "student")]
    [InlineData("teacher-attachment", "student")]
    [InlineData("student-attachment", "teacher")]
    [InlineData("student-attachment", "admin")]
    public async Task Wrong_role_is_rejected_without_opening_storage(string route, string role)
    {
        using var response = await GetFile(route, role, inline: true);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        AssertStorageNotOpened();
    }

    [Theory]
    [InlineData("student-report", "student")]
    [InlineData("teacher-report", "teacher")]
    [InlineData("student-attachment", "student")]
    [InlineData("teacher-attachment", "teacher")]
    public async Task Another_student_or_unrelated_teacher_cannot_read_the_file(string route, string role)
    {
        using var response = await GetFile(route, role, inline: true, userId: Guid.NewGuid());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertStorageNotOpened();
        Assert.DoesNotContain("storagePath", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("student-report", "student")]
    [InlineData("teacher-report", "teacher")]
    [InlineData("student-attachment", "student")]
    [InlineData("teacher-attachment", "teacher")]
    public async Task File_from_another_parent_resource_is_not_returned(string route, string role)
    {
        var path = PathFor(route)
            .Replace(LaboratoryId.ToString(), Guid.NewGuid().ToString())
            .Replace(ReportId.ToString(), Guid.NewGuid().ToString())
            .Replace(QuestionId.ToString(), Guid.NewGuid().ToString());
        using var request = AuthorizedRequest(path + "?inline=true", role);
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertStorageNotOpened();
    }

    [Theory]
    [InlineData("student-report", "student")]
    [InlineData("teacher-report", "teacher")]
    [InlineData("student-attachment", "student")]
    [InlineData("teacher-attachment", "teacher")]
    public async Task Missing_version_or_attachment_is_not_returned(string route, string role)
    {
        var path = PathFor(route)
            .Replace(VersionId.ToString(), Guid.NewGuid().ToString())
            .Replace(AttachmentId.ToString(), Guid.NewGuid().ToString());
        using var request = AuthorizedRequest(path + "?inline=true", role);
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertStorageNotOpened();
    }

    [Fact]
    public async Task Browser_can_request_a_range_of_an_authorized_pdf()
    {
        using var request = AuthorizedRequest(PathFor("student-report") + "?inline=true", "student");
        request.Headers.Range = new RangeHeaderValue(0, 3);
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(0, response.Content.Headers.ContentRange?.From);
        Assert.Equal(3, response.Content.Headers.ContentRange?.To);
        Assert.Equal(FileBytes.Length, response.Content.Headers.ContentRange?.Length);
        Assert.Equal(FileBytes[..4], await response.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("student-report", "student")]
    [InlineData("teacher-report", "teacher")]
    [InlineData("student-attachment", "student")]
    [InlineData("teacher-attachment", "teacher")]
    public async Task Small_range_reads_only_requested_bytes_from_minio(string route, string role)
    {
        const int length = 64 * 1024;
        _fileSizeBytes = 10 * 1024 * 1024;
        using var handler = new MinioTestHandler(new byte[_fileSizeBytes]);
        using var transport = new HttpClient(handler);
        var storage = handler.CreateStorage(transport);
        var reports = new LaboratoryReportFileStorage(storage);
        var attachments = new QuestionAttachmentStorage(storage);
        _reportStorage.Setup(x => x.CopyToAsync("reports/example", It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((string path, Stream destination, long offset, long count, CancellationToken ct) =>
                reports.CopyToAsync(path, destination, offset, count, ct));
        _attachmentStorage.Setup(x => x.CopyToAsync("attachments/example", It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((string path, Stream destination, long offset, long count, CancellationToken ct) =>
                attachments.CopyToAsync(path, destination, offset, count, ct));
        using var request = AuthorizedRequest(PathFor(route) + "?inline=true", role);
        request.Headers.Range = new RangeHeaderValue(length, 2 * length - 1);

        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(length, (await response.Content.ReadAsByteArrayAsync()).Length);
        Assert.Equal(length, handler.BytesRead);
        Assert.Equal($"bytes={length}-{2 * length - 1}", Assert.Single(handler.ObjectRanges));

        using var nextRequest = AuthorizedRequest(PathFor(route) + "?inline=true", role);
        nextRequest.Headers.Range = new RangeHeaderValue(2 * length, 3 * length - 1);
        using var nextResponse = await _client.SendAsync(nextRequest);

        Assert.Equal(HttpStatusCode.PartialContent, nextResponse.StatusCode);
        Assert.Equal(length, (await nextResponse.Content.ReadAsByteArrayAsync()).Length);
        Assert.Equal(2 * length, handler.BytesRead);
        Assert.Equal($"bytes={2 * length}-{3 * length - 1}", handler.ObjectRanges[1]);
        Assert.Equal(2, handler.ObjectRanges.Count);
    }

    [Theory]
    [InlineData("suffix")]
    [InlineData("open-ended")]
    [InlineData("past-end")]
    public async Task Normalized_range_is_passed_to_storage(string kind)
    {
        var (header, from, length) = kind switch
        {
            "suffix" => (new RangeHeaderValue(null, 4), FileBytes.Length - 4, 4),
            "open-ended" => (new RangeHeaderValue(4, null), 4, FileBytes.Length - 4),
            "past-end" => (new RangeHeaderValue(4, FileBytes.Length + 100), 4, FileBytes.Length - 4),
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        using var request = AuthorizedRequest(PathFor("student-report") + "?inline=true", "student");
        request.Headers.Range = header;
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
        Assert.Equal(FileBytes.AsSpan(from, length).ToArray(), await response.Content.ReadAsByteArrayAsync());
        _reportStorage.Verify(x => x.CopyToAsync("reports/example", It.IsAny<Stream>(), from, length, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("multiple")]
    [InlineData("malformed")]
    [InlineData("download")]
    public async Task Ignored_range_reads_the_whole_file_once(string kind)
    {
        using var request = AuthorizedRequest(PathFor("student-report") + (kind == "download" ? "" : "?inline=true"), "student");
        request.Headers.Range = new RangeHeaderValue(0, 3);
        if (kind == "multiple")
            request.Headers.Range.Ranges.Add(new RangeItemHeaderValue(8, 11));
        if (kind == "malformed")
        {
            request.Headers.Remove("Range");
            request.Headers.TryAddWithoutValidation("Range", "bytes=invalid");
        }
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(FileBytes, await response.Content.ReadAsByteArrayAsync());
        _reportStorage.Verify(x => x.CopyToAsync("reports/example", It.IsAny<Stream>(), 0, FileBytes.LongLength, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Empty_file_does_not_read_storage()
    {
        _fileSizeBytes = 0;
        using var response = await GetFile("student-report", "student", inline: true);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync());
        AssertStorageNotOpened();
    }

    [Fact]
    public async Task Cancelled_transfer_does_not_request_object_from_minio()
    {
        using var handler = new MinioTestHandler(FileBytes);
        using var transport = new HttpClient(handler);
        var storage = handler.CreateStorage(transport);
        using var destination = new MemoryStream();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            storage.CopyToAsync("reports/example", destination, 0, FileBytes.LongLength, cancellation.Token));

        Assert.Empty(handler.ObjectRanges);
        Assert.Equal(0, handler.BytesRead);
    }

    [Fact]
    public async Task Range_request_does_not_bypass_resource_authorization()
    {
        using var request = AuthorizedRequest(
            PathFor("student-report") + "?inline=true", "student", Guid.NewGuid());
        request.Headers.Range = new RangeHeaderValue(0, 3);
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertStorageNotOpened();
    }

    [Fact]
    public async Task Range_outside_the_file_returns_416()
    {
        using var request = AuthorizedRequest(PathFor("student-report") + "?inline=true", "student");
        request.Headers.Range = new RangeHeaderValue(FileBytes.Length + 1, null);
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
        Assert.Equal(FileBytes.Length, response.Content.Headers.ContentRange?.Length);
        AssertStorageNotOpened();
    }

    [Fact]
    public async Task Malformed_inline_parameter_returns_400_without_opening_storage()
    {
        using var request = AuthorizedRequest(PathFor("student-report") + "?inline=invalid", "student");
        using var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertStorageNotOpened();
    }

    private LaboratoryReportFileDto ReportFile() => new()
    {
        StoragePath = "reports/example",
        OriginalFileName = _fileName,
        ContentType = _contentType,
        FileSizeBytes = _fileSizeBytes
    };

    private QuestionDetailDto Question() => new()
    {
        Id = QuestionId,
        Status = QuestionStatus.Closed,
        Messages = [new QuestionReplyDto
        {
            Attachments = [new QuestionAttachmentDto
            {
                Id = AttachmentId,
                FileName = _fileName,
                ContentType = _contentType,
                SizeBytes = _fileSizeBytes,
                StoragePath = "attachments/example"
            }]
        }]
    };

    private async Task<HttpResponseMessage> GetFile(
        string route, string role, bool inline = false, bool cookie = false, Guid? userId = null)
    {
        using var request = AuthorizedRequest(PathFor(route) + (inline ? "?inline=true" : ""), role, userId, cookie);
        return await _client.SendAsync(request);
    }

    private static HttpRequestMessage AuthorizedRequest(string path, string role, Guid? userId = null, bool cookie = false)
    {
        var subject = userId ?? (role == "student" ? StudentId : TeacherId);
        var jwt = new JwtService(Options.Create(JwtSettings), new HttpContextAccessor());
        var token = jwt.GenerateAccessToken(new TokenUserDataDto(subject, "tests", Guid.NewGuid(), [], [role]));
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (cookie)
            request.Headers.Add("Cookie", $"access_token={token.AccessToken}");
        else
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return request;
    }

    private static string PathFor(string route) => route switch
    {
        "student-report" => $"/public/api/v1/laboratories/{LaboratoryId}/reports/my/versions/{VersionId}/file",
        "teacher-report" => $"/public/api/v1/teacher/reports/{ReportId}/versions/{VersionId}/file",
        "student-attachment" => $"/public/api/v1/questions/{QuestionId}/attachments/{AttachmentId}",
        "teacher-attachment" => $"/public/api/v1/teacher/questions/{QuestionId}/attachments/{AttachmentId}",
        _ => throw new ArgumentOutOfRangeException(nameof(route))
    };

    private void AssertStorageNotOpened()
    {
        _reportStorage.Verify(x => x.CopyToAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        _attachmentStorage.Verify(x => x.CopyToAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public void Dispose()
    {
        _client.Dispose();
        _host.Dispose();
    }
}
