using Application.Abstractions.Services.Groups;
using Application.Abstractions.Services.Groups.Contracts;
using Application.DTO.Groups;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Auth.Admin.Constants;

namespace Api.Controllers;

/// <summary>
/// Управление академическими группами (только для администраторов)
/// </summary>
[ApiController]
[Authorize(Roles = AuthRoleNames.AdminOrSuperAdmin)]
[Route("api/v1/admin/groups")]
public sealed class GroupController : PublicController
{
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    /// <summary>Получить список всех групп</summary>
    [HttpGet]
    public async Task<IActionResult> GetGroups(CancellationToken cancellationToken)
    {
        var groups = await _groupService.GetGroupsAsync(cancellationToken);
        return Ok(groups);
    }

    /// <summary>Получить детали группы вместе с участниками</summary>
    [HttpGet("{groupId:guid}")]
    public async Task<IActionResult> GetGroup(Guid groupId, CancellationToken cancellationToken)
    {
        try
        {
            var group = await _groupService.GetGroupDetailsAsync(groupId, cancellationToken);
            return Ok(group);
        }
        catch (GroupException ex)
        {
            return NotFound(new { ex.Code, Message = ex.Message });
        }
    }

    /// <summary>Создать группу</summary>
    [HttpPost]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateGroupRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var id = await _groupService.CreateGroupAsync(request, cancellationToken);
            return Ok(new { id });
        }
        catch (GroupException ex)
        {
            return BadRequest(new { ex.Code, Message = ex.Message });
        }
    }

    /// <summary>Переименовать группу</summary>
    [HttpPut("{groupId:guid}")]
    public async Task<IActionResult> RenameGroup(
        Guid groupId,
        [FromBody] CreateGroupRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _groupService.RenameGroupAsync(groupId, request.Name, cancellationToken);
            return NoContent();
        }
        catch (GroupException ex)
        {
            return BadRequest(new { ex.Code, Message = ex.Message });
        }
    }

    /// <summary>Удалить группу</summary>
    [HttpDelete("{groupId:guid}")]
    public async Task<IActionResult> DeleteGroup(Guid groupId, CancellationToken cancellationToken)
    {
        await _groupService.DeleteGroupAsync(groupId, cancellationToken);
        return NoContent();
    }

    /// <summary>Получить студентов, не состоящих ни в одной группе</summary>
    [HttpGet("ungrouped-students")]
    public async Task<IActionResult> GetUngroupedStudents(CancellationToken cancellationToken)
    {
        var students = await _groupService.GetUngroupedStudentsAsync(cancellationToken);
        return Ok(students);
    }

    /// <summary>Добавить студента в группу</summary>
    [HttpPost("{groupId:guid}/students/{userId:guid}")]
    public async Task<IActionResult> AddStudent(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        await _groupService.AddStudentToGroupAsync(groupId, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>Добавить нескольких студентов в группу</summary>
    [HttpPost("{groupId:guid}/students/bulk")]
    public async Task<IActionResult> AddStudents(Guid groupId, [FromBody] BulkMembersRequest request, CancellationToken cancellationToken)
    {
        await _groupService.AddStudentsToGroupAsync(groupId, request.UserIds, cancellationToken);
        return NoContent();
    }

    /// <summary>Убрать студента из группы</summary>
    [HttpDelete("{groupId:guid}/students/{userId:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        await _groupService.RemoveStudentFromGroupAsync(groupId, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>Назначить преподавателя на группу</summary>
    [HttpPost("{groupId:guid}/teachers/{userId:guid}")]
    public async Task<IActionResult> AddTeacher(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        await _groupService.AddTeacherToGroupAsync(groupId, userId, cancellationToken);
        return NoContent();
    }

    /// <summary>Назначить нескольких преподавателей на группу</summary>
    [HttpPost("{groupId:guid}/teachers/bulk")]
    public async Task<IActionResult> AddTeachers(Guid groupId, [FromBody] BulkMembersRequest request, CancellationToken cancellationToken)
    {
        await _groupService.AddTeachersToGroupAsync(groupId, request.UserIds, cancellationToken);
        return NoContent();
    }

    /// <summary>Снять преподавателя с группы</summary>
    [HttpDelete("{groupId:guid}/teachers/{userId:guid}")]
    public async Task<IActionResult> RemoveTeacher(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        await _groupService.RemoveTeacherFromGroupAsync(groupId, userId, cancellationToken);
        return NoContent();
    }
}
