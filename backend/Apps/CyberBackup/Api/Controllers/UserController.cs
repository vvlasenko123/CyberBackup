using Api.Controllers.Models.Request;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO;
using AutoMapper;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Контроллер с пользователями
/// </summary>
[ApiController]
[Route("user")]
public class UserController : PublicController
{
    private readonly IMapper _mapper;
    private readonly ICreateUserUseCaseManager _createUserUseCaseManager;

    /// <summary>
    /// флоу
    /// 1) из токена мы получаем текущего пользователя
    /// 2) исходя из роли определяем может ли создать пользователь другого пользователя
    /// </summary>
    public UserController(IMapper mapper, ICreateUserUseCaseManager createUserUseCaseManager)
    {
        _mapper = mapper;
        _createUserUseCaseManager = createUserUseCaseManager;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request, CancellationToken token)
    {
        var dto = _mapper.Map<UserDto>(request);
        await _createUserUseCaseManager.Execute(dto, token);

        return Created();
    }
}