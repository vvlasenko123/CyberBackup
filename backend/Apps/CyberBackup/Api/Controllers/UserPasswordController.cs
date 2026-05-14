using Api.Controllers.Models.Request;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO.Auth;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер пароля пользователя
/// </summary>
[ApiController]
[Route("user/password")]
[Authorize]
public sealed class UserPasswordController : PublicController
{
    private readonly IMapper _mapper;
    private readonly IChangePasswordUseCaseManager _changePasswordUseCaseManager;

    public UserPasswordController(
        IMapper mapper,
        IChangePasswordUseCaseManager changePasswordUseCaseManager)
    {
        _mapper = mapper;
        _changePasswordUseCaseManager = changePasswordUseCaseManager;
    }

    /// <summary>
    /// Сменить пароль текущего пользователя
    /// </summary>
    [HttpPost("change")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken token)
    {
        var dto = _mapper.Map<ChangePasswordDto>(request);

        await _changePasswordUseCaseManager.Execute(dto, token);

        return NoContent();
    }
}