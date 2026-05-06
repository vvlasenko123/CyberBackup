using Api.Controllers.Models.Request;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.DTO.Auth;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
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

    public AuthController(IMapper mapper, IRegisterUseCaseManager registerUseCaseManager)
    {
        _mapper = mapper;
        _registerUseCaseManager = registerUseCaseManager;
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
}