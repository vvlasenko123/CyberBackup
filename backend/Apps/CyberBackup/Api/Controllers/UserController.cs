using Api.Controllers.Models.Request;
using Api.Controllers.Models.Response;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO;
using Application.DTO.User;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Контроллер с пользователями.
/// </summary>
[ApiController]
[Route("user")]
public class UserController : PublicController
{
    private readonly IMapper _mapper;
    private readonly ICreateUserUseCaseManager _createUserUseCaseManager;
    private readonly IUpdateUserUseCaseManager _updateUserUseCaseManager;
    private readonly IDeleteUserUseCaseManager _deleteUserUseCaseManager;
    private readonly IGetUserUseCaseManager _getUserUseCaseManager;

    public UserController(
        IMapper mapper,
        ICreateUserUseCaseManager createUserUseCaseManager,
        IUpdateUserUseCaseManager updateUserUseCaseManager,
        IDeleteUserUseCaseManager deleteUserUseCaseManager,
        IGetUserUseCaseManager getUserUseCaseManager)
    {
        _mapper = mapper;
        _createUserUseCaseManager = createUserUseCaseManager;
        _updateUserUseCaseManager = updateUserUseCaseManager;
        _deleteUserUseCaseManager = deleteUserUseCaseManager;
        _getUserUseCaseManager = getUserUseCaseManager;
    }

    /// <summary>
    /// Создать пользователя
    /// </summary>
    [Authorize(Roles = AuthRoleNames.AdminOrSuperAdmin)]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(
        CreateUserRequest request,
        CancellationToken token)
    {
        var dto = _mapper.Map<UserDto>(request);

        await _createUserUseCaseManager.Execute(dto, token);

        return Created();
    }

    /// <summary>
    /// Изменить пользователя
    /// </summary>
    [Authorize(Roles = AuthRoleNames.AdminOrSuperAdmin)]
    [HttpPut("update/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken token)
    {
        var dto = _mapper.Map<UpdateUserDto>(request) with
        {
            Id = id
        };

        await _updateUserUseCaseManager.Execute(dto, token);

        return NoContent();
    }

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    [Authorize(Roles = AuthRoleNames.AdminOrSuperAdmin)]
    [HttpDelete("delete/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken token)
    {
        await _deleteUserUseCaseManager.Execute(id, token);

        return NoContent();
    }

    /// <summary>
    /// Получить пользователя
    /// </summary>
    [Authorize]
    [HttpGet("get/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken token)
    {
        var user = await _getUserUseCaseManager.Execute(id, token);

        if (user is null)
        {
            return NotFound(new
            {
                Message = "Пользователь не найден"
            });
        }

        var response = _mapper.Map<UserResponse>(user);

        return Ok(response);
    }

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    [Authorize(Roles = AuthRoleNames.AdminOrSuperAdmin)]
    [HttpGet("get-all")]
    public async Task<IActionResult> GetAllUsers(CancellationToken token)
    {
        var users = await _getUserUseCaseManager.Execute(token);
        var response = _mapper.Map<IReadOnlyCollection<UserResponse>>(users);

        return Ok(response);
    }
}