using Api.Controllers.Models.Request;
using Application.Abstractions.UseCases.Auth.Contracts;
using Application.DTO.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер авторизации.
/// </summary>
[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IRegisterUseCase _registerUseCase;

    public AuthController(IRegisterUseCase registerUseCase)
    {
        _registerUseCase = registerUseCase;
    }

    /// <summary>
    /// Регистрация пользователя.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registerUseCase.Execute(
            new RegisterRequestDto(
                Email: request.Email,
                FullName: request.FullName,
                Password: request.Password),
            cancellationToken);

        return Ok(new
        {
            result.UserId,
            Role = result.Role.ToString(),
            result.AccessToken,

            TokenMetadata = new
            {
                SubjectId = result.UserId.ToString(),
                result.JwtId,
                result.ClientId,
                result.SessionId,
                result.Scopes,
                result.Roles,
                result.IssuedAtUtc,
                result.ExpiresAtUtc
            }
        });
    }
}