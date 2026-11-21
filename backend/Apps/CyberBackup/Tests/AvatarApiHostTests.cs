using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Api.Controllers;
using Api.Errors;
using Api.Mappings;
using Api.Services.Avatars;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Avatars;
using Application.Abstractions.Services.Avatars.Contracts;
using Application.Abstractions.Services.UserProfiles;
using Application.Abstractions.Services.UserProfiles.Contracts;
using Application.DTO.Auth;
using Domain.Repositories;
using Domain.User.Enums;
using Infrastructure.AutoMapper;
using Infrastructure.Core.Controllers.Convention;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tests.Support;

namespace Tests;

/// <summary>
/// Контроллеры аватара и профиля на реальном Kestrel с теми же лимитами, что в продакшене,
/// глобальным обработчиком ошибок и фейковыми хранилищем и базой
/// </summary>
public sealed class ApiHostFixture : IAsyncLifetime
{
    private WebApplication? _app;

    internal FakeUserRepository Repository { get; } = new();

    internal FakeAvatarStorage Storage { get; } = new();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel(options =>
        {
            // Как в KestrelOptionsExtension: глобальный лимит 1 МБ, эндпоинт загрузки переопределяет его атрибутом
            options.Limits.MaxRequestBodySize = 1 * 1024 * 1024;
            options.Listen(System.Net.IPAddress.Loopback, 0);
        });

        builder.Services
            .AddControllers(options => options.Conventions.Add(new BaseControllerRouteConvention()))
            .AddApplicationPart(typeof(UserAvatarController).Assembly);

        builder.Services.AddCyberMapper(typeof(UserMapper).Assembly);
        builder.Services.AddHttpContextAccessor();
        builder.Services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, HeaderAuthHandler>("Test", _ => { });
        builder.Services.AddAuthorization();

        builder.Services.AddSingleton<IUserRepository>(Repository);
        builder.Services.AddSingleton<IUserAvatarStorage>(Storage);
        builder.Services.AddSingleton<IAvatarImageProcessor, AvatarImageProcessor>();
        builder.Services.AddScoped<IJwtService, HeaderJwtService>();
        builder.Services.AddScoped<IAvatarUploadRequestFactory, AvatarUploadRequestFactory>();
        builder.Services.AddScoped<IUserAvatarService, UserAvatarService>();
        builder.Services.AddScoped<IUserProfileService, UserProfileService>();

        _app = builder.Build();
        _app.UseExceptionHandler(errorApp => errorApp.Run(ApiExceptionHandler.HandleAsync));
        _app.UseRouting();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();

        await _app.StartAsync();

        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        Client = new HttpClient { BaseAddress = new Uri(address) };
    }

    public async Task DisposeAsync()
    {
        Client.Dispose();

        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }

    private sealed class HeaderAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public HeaderAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Test-User", out var user))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var role = Request.Headers.TryGetValue("X-Test-Role", out var value) ? value.ToString() : "student";
            var identity = new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, user.ToString()), new Claim(ClaimTypes.Role, role)],
                "Test");

            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(new ClaimsPrincipal(identity), "Test")));
        }
    }

    private sealed class HeaderJwtService : IJwtService
    {
        private readonly IHttpContextAccessor _accessor;

        public HeaderJwtService(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public GeneratedAccessTokenDto GenerateAccessToken(TokenUserDataDto userData) => throw new NotSupportedException();

        public CurrentTokenUserDto GetCurrentUser()
        {
            var headers = _accessor.HttpContext!.Request.Headers;
            var role = headers["X-Test-Role"].ToString() switch
            {
                "admin" => UserRole.Admin,
                "superadmin" => UserRole.SuperAdmin,
                "teacher" => UserRole.Teacher,
                _ => UserRole.Student
            };

            return new CurrentTokenUserDto(Guid.Parse(headers["X-Test-User"].ToString()), role);
        }
    }
}

public sealed class AvatarApiHostTests : IClassFixture<ApiHostFixture>
{
    private const string Prefix = "/public/api/v1/user";

    private readonly ApiHostFixture _host;

    public AvatarApiHostTests(ApiHostFixture host)
    {
        _host = host;
    }

    private HttpRequestMessage Request(HttpMethod method, string url, Guid? user, string role = "student", HttpContent? content = null)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };

        if (user is not null)
        {
            request.Headers.Add("X-Test-User", user.Value.ToString());
            request.Headers.Add("X-Test-Role", role);
        }

        return request;
    }

    private static MultipartFormDataContent File(byte[] bytes, string fileName, string contentType, string field = "file")
    {
        var part = new ByteArrayContent(bytes);
        part.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        return new MultipartFormDataContent { { part, field, fileName } };
    }

    private async Task<HttpResponseMessage> Upload(Guid user, byte[] bytes, string fileName = "a.png", string contentType = "image/png")
    {
        return await _host.Client.SendAsync(
            Request(HttpMethod.Put, $"{Prefix}/me/avatar", user, content: File(bytes, fileName, contentType)));
    }

    private static async Task<JsonElement> Json(HttpResponseMessage response)
    {
        return JsonSerializer.Deserialize<JsonElement>(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Upload_then_read_returns_image_with_required_headers()
    {
        var user = _host.Repository.AddUser();
        var reader = _host.Repository.AddUser();

        var upload = await Upload(user, TestImages.Png(120, 120));
        Assert.Equal(HttpStatusCode.OK, upload.StatusCode);
        Assert.True((await Json(upload)).TryGetProperty("avatarUpdatedAt", out _));

        var response = await _host.Client.SendAsync(Request(HttpMethod.Get, $"{Prefix}/{user}/avatar", reader));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/png", response.Content.Headers.ContentType!.MediaType);
        Assert.True(response.Headers.CacheControl!.Private);
        Assert.True(response.Headers.CacheControl.NoCache);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType ?? "inline");
        Assert.NotNull(response.Headers.ETag);
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, (await response.Content.ReadAsByteArrayAsync())[..4]);
    }

    [Fact]
    public async Task Conditional_request_gets_304_without_body_and_without_reading_storage()
    {
        var user = _host.Repository.AddUser();
        await Upload(user, TestImages.Png(120, 120));

        var first = await _host.Client.SendAsync(Request(HttpMethod.Get, $"{Prefix}/{user}/avatar", user));
        var etag = first.Headers.ETag!.ToString();
        var readsBefore = _host.Storage.OpenReadCalls;

        var request = Request(HttpMethod.Get, $"{Prefix}/{user}/avatar", user);
        request.Headers.TryAddWithoutValidation("If-None-Match", etag);
        var second = await _host.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        Assert.Empty(await second.Content.ReadAsByteArrayAsync());
        Assert.Equal(etag, second.Headers.ETag!.ToString());
        Assert.Equal(readsBefore, _host.Storage.OpenReadCalls);
    }

    [Fact]
    public async Task Request_over_two_megabytes_gets_413_not_500_or_400()
    {
        var user = _host.Repository.AddUser();

        // Expect: 100-continue: сервер отвечает 413 по Content-Length до того, как клиент начнёт слать тело,
        // иначе клиент может получить обрыв соединения вместо ответа
        var request = Request(
            HttpMethod.Put,
            $"{Prefix}/me/avatar",
            user,
            content: File(new byte[3 * 1024 * 1024], "a.png", "image/png"));
        request.Headers.ExpectContinue = true;

        var response = await _host.Client.SendAsync(request);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.True((await Json(response)).TryGetProperty("message", out _));
        Assert.Empty(_host.Storage.Keys.Where(key => key.Contains(user.ToString("N"))));
    }

    [Fact]
    public async Task File_between_one_and_two_megabytes_is_rejected_by_the_avatar_rules_not_by_the_request_limit()
    {
        var user = _host.Repository.AddUser();

        var response = await Upload(user, new byte[1_500_000]);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("avatar.too_large", (await Json(response)).GetProperty("code").GetString());
    }

    [Theory]
    [InlineData("gif", "image/gif", "avatar.invalid_format")]
    [InlineData("png", "image/jpeg", "avatar.invalid_format")]
    public async Task Avatar_errors_are_400_with_code_and_message(string extension, string contentType, string expectedCode)
    {
        var user = _host.Repository.AddUser();
        var bytes = extension == "gif" ? TestImages.Gif() : TestImages.Png(100, 100);

        var response = await Upload(user, bytes, $"a.{extension}", contentType);
        var body = await Json(response);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedCode, body.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(body.GetProperty("message").GetString()));
    }

    [Fact]
    public async Task Missing_file_field_gives_file_required()
    {
        var user = _host.Repository.AddUser();

        var response = await _host.Client.SendAsync(Request(
            HttpMethod.Put,
            $"{Prefix}/me/avatar",
            user,
            content: File(TestImages.Png(100, 100), "a.png", "image/png", field: "other")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("avatar.file_required", (await Json(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Non_multipart_request_is_415()
    {
        var user = _host.Repository.AddUser();

        var response = await _host.Client.SendAsync(Request(
            HttpMethod.Put,
            $"{Prefix}/me/avatar",
            user,
            content: JsonContent.Create(new { })));

        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task Requests_without_authentication_are_401()
    {
        var anyone = Guid.NewGuid();

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _host.Client.SendAsync(Request(HttpMethod.Get, $"{Prefix}/{anyone}/avatar", null))).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _host.Client.SendAsync(Request(HttpMethod.Put, $"{Prefix}/me/avatar", null, content: File([1], "a.png", "image/png")))).StatusCode);
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/me/avatar", null))).StatusCode);
    }

    [Fact]
    public async Task Read_gives_404_for_missing_user_and_missing_avatar()
    {
        var user = _host.Repository.AddUser();

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _host.Client.SendAsync(Request(HttpMethod.Get, $"{Prefix}/{user}/avatar", user))).StatusCode);
        Assert.Equal(
            HttpStatusCode.NotFound,
            (await _host.Client.SendAsync(Request(HttpMethod.Get, $"{Prefix}/{Guid.NewGuid()}/avatar", user))).StatusCode);
    }

    [Fact]
    public async Task Own_removal_is_204_and_idempotent_and_404_for_deleted_user()
    {
        var user = _host.Repository.AddUser();
        await Upload(user, TestImages.Png(100, 100));

        var first = await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/me/avatar", user));
        var second = await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/me/avatar", user));

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);

        _host.Repository.RemoveUser(user);
        var gone = await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/me/avatar", user));
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task Upload_for_a_user_deleted_after_login_is_404()
    {
        var user = _host.Repository.AddUser();
        _host.Repository.RemoveUser(user);

        var response = await Upload(user, TestImages.Png(100, 100));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(_host.Storage.Keys.Where(key => key.Contains(user.ToString("N"))));
    }

    [Fact]
    public async Task Removing_someone_elses_avatar_is_admin_only()
    {
        var owner = _host.Repository.AddUser();
        var other = _host.Repository.AddUser();
        await Upload(owner, TestImages.Png(100, 100));

        var byStudent = await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/{owner}/avatar", other));
        var byTeacher = await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/{owner}/avatar", other, "teacher"));
        var byAdmin = await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/{owner}/avatar", other, "admin"));

        Assert.Equal(HttpStatusCode.Forbidden, byStudent.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, byTeacher.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, byAdmin.StatusCode);
        Assert.Null(_host.Repository.AvatarPathOf(owner));

        var missing = await _host.Client.SendAsync(Request(HttpMethod.Delete, $"{Prefix}/{Guid.NewGuid()}/avatar", other, "superadmin"));
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    // ---------- профиль ----------

    [Fact]
    public async Task Profile_update_normalizes_contacts_and_get_returns_them()
    {
        var user = _host.Repository.AddUser();

        var put = await _host.Client.SendAsync(Request(
            HttpMethod.Put,
            $"{Prefix}/me",
            user,
            content: JsonContent.Create(new { phone = "+7 (900) 123-45-67", telegram = "@student_one", vk = "https://vk.com/student1" })));
        var get = await _host.Client.SendAsync(Request(HttpMethod.Get, $"{Prefix}/me", user));

        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        var body = await Json(get);
        Assert.Equal("+79001234567", body.GetProperty("phone").GetString());
        Assert.Equal("student_one", body.GetProperty("telegram").GetString());
        Assert.Equal("student1", body.GetProperty("vk").GetString());
        Assert.Equal(JsonValueKind.Null, body.GetProperty("avatarUpdatedAt").ValueKind);
    }

    [Fact]
    public async Task Profile_update_with_invalid_contact_is_400_with_code()
    {
        var user = _host.Repository.AddUser();

        var response = await _host.Client.SendAsync(Request(
            HttpMethod.Put,
            $"{Prefix}/me",
            user,
            content: JsonContent.Create(new { phone = "123" })));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("profile.phone_invalid", (await Json(response)).GetProperty("code").GetString());
    }

    [Fact]
    public async Task Profile_requires_authentication()
    {
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            (await _host.Client.SendAsync(Request(HttpMethod.Get, $"{Prefix}/me", null))).StatusCode);
    }
}
