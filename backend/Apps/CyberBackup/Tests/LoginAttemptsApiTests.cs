using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Controllers;
using Api.Errors;
using Api.Mappings;
using Application.Abstractions.Services.Auth;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.Abstractions.UseCases.User;
using Application.Abstractions.UseCases.User.Contracts;
using Application.Features.Auth.Login;
using Domain.Repositories;
using Infrastructure.AutoMapper;
using Infrastructure.Core.Controllers.Convention;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tests.Support;

namespace Tests;

/// <summary>
/// Вход и смена пароля через настоящие контроллеры, сценарии, лимитер и глобальный обработчик ошибок;
/// база и хэширование паролей заменены фейками, время задаётся тестом
/// </summary>
public sealed class LoginAttemptsFixture : IAsyncLifetime
{
    private WebApplication? _app;

    internal ManualTimeProvider Time { get; } = new();

    internal CredentialsUserRepository Users { get; } = new();

    internal CountingPasswordHashService Hasher { get; } = new();

    internal StubJwtService Jwt { get; } = new();

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 0));

        builder.Services
            .AddControllers(options => options.Conventions.Add(new BaseControllerRouteConvention()))
            .AddApplicationPart(typeof(AuthController).Assembly);

        builder.Services.AddCyberMapper(typeof(UserMapper).Assembly);
        builder.Services.AddHttpContextAccessor();
        builder.Services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, AlwaysAuthenticatedHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton<ICookieManager, ChunkingCookieManager>();
        builder.Services.AddScoped<Api.Auth.AppendLoginCookiesFilter>();

        builder.Services.AddSingleton<TimeProvider>(Time);
        builder.Services.AddSingleton(PasswordAttemptPolicy.Default);
        builder.Services.AddSingleton<ILoginAttemptRepository, InMemoryLoginAttemptRepository>();
        builder.Services.AddScoped<IPasswordAttemptLimiter, PasswordAttemptLimiter>();
        builder.Services.AddSingleton<IUserRepository>(Users);
        builder.Services.AddSingleton<IRefreshTokenRepository, StubRefreshTokenRepository>();
        builder.Services.AddSingleton<IPasswordHashService>(Hasher);
        builder.Services.AddSingleton<IJwtService>(Jwt);
        builder.Services.AddSingleton<IRefreshTokenService, StubRefreshTokenService>();
        builder.Services.AddSingleton<IAuthTokenDefaultsService, StubAuthTokenDefaults>();
        builder.Services.AddScoped<ILoginUseCaseManager, LoginUseCaseManager>();
        builder.Services.AddScoped<IChangePasswordUseCaseManager, ChangePasswordUseCaseManager>();
        builder.Services.AddScoped<IRegisterUseCaseManager, UnusedRegisterUseCaseManager>();

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

    /// <summary>
    /// Пользователь определяется <see cref="StubJwtService"/>, схема аутентификации лишь пропускает запрос
    /// </summary>
    private sealed class AlwaysAuthenticatedHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AlwaysAuthenticatedHandler(
            Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new System.Security.Claims.ClaimsIdentity("Test");

            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(
                new System.Security.Claims.ClaimsPrincipal(identity), "Test")));
        }
    }

    private sealed class UnusedRegisterUseCaseManager : IRegisterUseCaseManager
    {
        public Task<Application.DTO.Auth.RegisterResultDto> Execute(
            Application.DTO.Auth.RegisterRequestDto request,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}

public sealed class LoginAttemptsApiTests : IClassFixture<LoginAttemptsFixture>
{
    private const string Password = "correct horse";

    private readonly LoginAttemptsFixture _host;

    public LoginAttemptsApiTests(LoginAttemptsFixture host)
    {
        _host = host;
    }

    [Fact]
    public async Task Wrong_password_returns_401_with_remaining_attempts()
    {
        var email = NewUser();

        var first = await Login(email, "wrong");
        var second = await Login(email, "wrong");

        Assert.Equal(HttpStatusCode.Unauthorized, first.Status);
        Assert.Equal("auth.invalid_credentials", first.Json.GetProperty("code").GetString());
        Assert.Equal(4, first.Json.GetProperty("remainingAttempts").GetInt32());
        Assert.Equal(JsonValueKind.Null, first.Json.GetProperty("retryAfterSeconds").ValueKind);
        Assert.Contains("Осталось попыток: 4", first.Json.GetProperty("message").GetString());
        Assert.Equal(3, second.Json.GetProperty("remainingAttempts").GetInt32());
    }

    [Fact]
    public async Task Fifth_wrong_attempt_reports_unlock_time_and_next_attempts_get_429()
    {
        var email = NewUser();
        (HttpStatusCode Status, JsonElement Json, HttpResponseMessage Response) last = default;

        for (var i = 0; i < 5; i++)
        {
            last = await Login(email, "wrong");
        }

        Assert.Equal(HttpStatusCode.Unauthorized, last.Status);
        Assert.Equal(0, last.Json.GetProperty("remainingAttempts").GetInt32());
        Assert.Equal(30, last.Json.GetProperty("retryAfterSeconds").GetInt32());
        Assert.Equal(_host.Time.GetUtcNow().AddSeconds(30), last.Json.GetProperty("unlockAtUtc").GetDateTimeOffset());

        _host.Time.Advance(TimeSpan.FromSeconds(10));
        var blocked = await Login(email, "wrong");

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.Status);
        Assert.Equal("auth.rate_limited", blocked.Json.GetProperty("code").GetString());
        Assert.Equal(0, blocked.Json.GetProperty("remainingAttempts").GetInt32());
        Assert.Equal(20, blocked.Json.GetProperty("retryAfterSeconds").GetInt32());
        Assert.Equal("20", Assert.Single(blocked.Response.Headers.GetValues("Retry-After")));
    }

    [Fact]
    public async Task Correct_password_is_not_even_checked_while_locked()
    {
        var email = NewUser();

        for (var i = 0; i < 5; i++)
        {
            await Login(email, "wrong");
        }

        var verifyCalls = _host.Hasher.VerifyCalls;
        var blocked = await Login(email, Password);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.Status);
        Assert.Equal(verifyCalls, _host.Hasher.VerifyCalls);

        _host.Time.Advance(TimeSpan.FromSeconds(30));
        var allowed = await Login(email, Password);

        Assert.Equal(HttpStatusCode.OK, allowed.Status);
    }

    [Fact]
    public async Task Successful_login_resets_the_counter()
    {
        var email = NewUser();

        await Login(email, "wrong");
        await Login(email, "wrong");
        Assert.Equal(HttpStatusCode.OK, (await Login(email, Password)).Status);

        var next = await Login(email, "wrong");

        Assert.Equal(4, next.Json.GetProperty("remainingAttempts").GetInt32());
    }

    [Fact]
    public async Task Unknown_email_is_limited_and_answers_like_a_wrong_password()
    {
        var known = NewUser();
        var unknown = $"nobody-{Guid.NewGuid():N}@example.com";

        var knownAnswer = await Login(known, "wrong");
        var unknownAnswer = await Login(unknown, "wrong");

        Assert.Equal(knownAnswer.Status, unknownAnswer.Status);
        Assert.Equal(
            knownAnswer.Json.GetProperty("remainingAttempts").GetInt32(),
            unknownAnswer.Json.GetProperty("remainingAttempts").GetInt32());

        for (var i = 0; i < 4; i++)
        {
            await Login(unknown, "wrong");
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, (await Login(unknown, "wrong")).Status);
    }

    [Fact]
    public async Task Email_case_does_not_give_extra_attempts()
    {
        var email = NewUser();

        for (var i = 0; i < 5; i++)
        {
            await Login(i % 2 == 0 ? email : email.ToUpperInvariant(), "wrong");
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, (await Login(email.ToUpperInvariant(), "wrong")).Status);
    }

    [Fact]
    public async Task Other_accounts_are_not_affected()
    {
        var locked = NewUser();
        var other = NewUser();

        for (var i = 0; i < 5; i++)
        {
            await Login(locked, "wrong");
        }

        Assert.Equal(HttpStatusCode.OK, (await Login(other, Password)).Status);
    }

    [Fact]
    public async Task Change_password_is_limited_by_user()
    {
        var email = NewUser();
        var user = _host.Users.GetByEmailAsync(email, default).Result!;
        _host.Jwt.CurrentUserId = user.Id;

        HttpResponseMessage response = null!;

        for (var i = 0; i < 4; i++)
        {
            response = await ChangePassword("wrong");
        }

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("auth.invalid_password", body.GetProperty("code").GetString());
        Assert.Equal(1, body.GetProperty("remainingAttempts").GetInt32());

        await ChangePassword("wrong");
        var blocked = await ChangePassword(Password);

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
        Assert.NotNull(blocked.Headers.RetryAfter);
        Assert.Equal(0, _host.Users.ChangePasswordCalls);
    }

    private string NewUser()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        _host.Users.Add(email, "hash:" + Password);

        return email;
    }

    private async Task<(HttpStatusCode Status, JsonElement Json, HttpResponseMessage Response)> Login(
        string email,
        string password)
    {
        var response = await _host.Client.PostAsJsonAsync("/public/auth/login", new { email, password });
        var text = await response.Content.ReadAsStringAsync();
        var json = text.StartsWith('{') ? JsonDocument.Parse(text).RootElement.Clone() : default;

        return (response.StatusCode, json, response);
    }

    private Task<HttpResponseMessage> ChangePassword(string currentPassword)
    {
        return _host.Client.PostAsJsonAsync(
            "/public/user/password/change",
            new { currentPassword, newPassword = "brand new password" });
    }
}
