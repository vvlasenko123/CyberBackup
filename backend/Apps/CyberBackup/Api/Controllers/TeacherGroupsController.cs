using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.Groups.Contracts;
using Domain.User.Enums;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>Группы, доступные для назначения лабораторных.</summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.Teacher + "," + AuthRoleNames.AdminOrSuperAdmin)]
[Route("api/v1/teacher/groups")]
public sealed class TeacherGroupsController(IGroupService groups, IJwtService jwtService) : PublicController
{
    /// <summary>Получить свои группы; администраторы получают все группы.</summary>
    [HttpGet]
    public async Task<IActionResult> GetGroups(CancellationToken cancellationToken)
    {
        var user = jwtService.GetCurrentUser();
        return Ok(await groups.GetTeacherGroupsAsync(user.UserId,
            user.Role is UserRole.Admin or UserRole.SuperAdmin, cancellationToken));
    }
}
