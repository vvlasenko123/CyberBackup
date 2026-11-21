using Api.Controllers.Models.Request;
using Api.Controllers.Models.Response;
using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.UserProfiles.Contracts;
using Application.DTO.UserProfiles;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Собственный профиль пользователя: чтение и обновление контактов
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/user/me")]
public sealed class UserProfileController : PublicController
{
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    private readonly IUserProfileService _profileService;

    public UserProfileController(
        IMapper mapper,
        IJwtService jwtService,
        IUserProfileService profileService)
    {
        _mapper = mapper;
        _jwtService = jwtService;
        _profileService = profileService;
    }

    /// <summary>
    /// Получить свой профиль
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMe(CancellationToken token)
    {
        var currentUser = _jwtService.GetCurrentUser();
        var user = await _profileService.GetAsync(currentUser.UserId, token);

        return Ok(_mapper.Map<UserResponse>(user));
    }

    /// <summary>
    /// Заменить свои контакты (телефон, Telegram, VK)
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateMe(UpdateProfileRequest request, CancellationToken token)
    {
        var currentUser = _jwtService.GetCurrentUser();
        var dto = new UpdateContactsDto
        {
            Phone = request.Phone,
            Telegram = request.Telegram,
            Vk = request.Vk
        };

        var user = await _profileService.UpdateContactsAsync(currentUser.UserId, dto, token);

        return Ok(_mapper.Map<UserResponse>(user));
    }
}
