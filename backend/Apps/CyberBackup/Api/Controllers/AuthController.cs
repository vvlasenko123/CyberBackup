using Api.Controllers.Models.Request;
using Api.Services.Auth;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.DTO.Auth;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Infrastructure.Exceptions.User;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер авторизации.
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController : PublicController
{
    private readonly IMapper _mapper;
    private readonly IRegisterUseCaseManager _registerUseCaseManager;
    private readonly ILoginUseCaseManager _loginUseCaseManager;
    private readonly IAuthCookieService _authCookieService;

    public AuthController(
        IMapper mapper,
        IRegisterUseCaseManager registerUseCaseManager,
        ILoginUseCaseManager loginUseCaseManager,
        IAuthCookieService authCookieService)
    {
        _mapper = mapper;
        _registerUseCaseManager = registerUseCaseManager;
        _loginUseCaseManager = loginUseCaseManager;
        _authCookieService = authCookieService;
    }

    /// <summary>
    /// Регистрация пользователя.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<RegisterRequestDto>(request);
        var result = await _registerUseCaseManager.Execute(dto, cancellationToken);

        return Ok(result);
    }
    
    /// <summary>
    /// Вход пользователя.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<LoginRequestDto>(request);
        var result = await _loginUseCaseManager.Execute(dto, cancellationToken);

        if (result is null)
        {
            return Unauthorized(new
            {
                Message = "Неверный email или пароль"
            });
        }

        _authCookieService.AppendAuthenticationCookies(
            Response,
            result.AccessToken,
            result.RefreshToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshTokenExpiresAtUtc);

        return Ok();
    }
}