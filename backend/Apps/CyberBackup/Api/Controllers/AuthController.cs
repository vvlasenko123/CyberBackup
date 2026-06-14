using Api.Auth;
using Api.Controllers.Models.Request;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.DTO.Auth;
using Application.Features.Auth.Login;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

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

    public AuthController(
        IMapper mapper,
        IRegisterUseCaseManager registerUseCaseManager,
        ILoginUseCaseManager loginUseCaseManager)
    {
        _mapper = mapper;
        _registerUseCaseManager = registerUseCaseManager;
        _loginUseCaseManager = loginUseCaseManager;
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
    [AppendLoginCookies]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var dto = _mapper.Map<LoginRequestDto>(request);
        LoginResultDto? result;
        try
        {
            result = await _loginUseCaseManager.Execute(dto, cancellationToken);
        }
        catch (AccountDeactivatedException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }

        if (result is null)
        {
            return Unauthorized(new { Message = SecurityText.InvalidEmailOrPassword });
        }

        return Ok(result);
    }
}