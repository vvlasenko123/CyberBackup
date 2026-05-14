using Domain.User.Enums;

namespace Application.DTO.User.Validate;

/// <summary>
/// Проверка прав на работу с ролями пользователей.
/// </summary>
public static class UserRolePermissionValidator
{
    /// <summary>
    /// Проверить возможность создать пользователя с ролью
    /// </summary>
    public static void ValidateCreate(UserRole currentUserRole, UserRole newUserRole)
    {
        if (newUserRole is UserRole.SuperAdmin)
        {
            throw new InvalidOperationException("Нельзя создать суперадминистратора через API");
        }

        if (currentUserRole is UserRole.SuperAdmin)
        {
            return;
        }

        if (currentUserRole is UserRole.Admin &&
            newUserRole is UserRole.Student or UserRole.Teacher)
        {
            return;
        }

        throw new InvalidOperationException("Недостаточно прав для создания пользователя с такой ролью");
    }

    /// <summary>
    /// Проверить возможность изменить пользователя с ролью
    /// </summary>
    public static void ValidateUpdate(UserRole currentUserRole, UserRole oldUserRole, UserRole newUserRole)
    {
        if (oldUserRole is UserRole.SuperAdmin || newUserRole is UserRole.SuperAdmin)
        {
            throw new InvalidOperationException("Нельзя изменить суперадминистратора через API");
        }

        if (currentUserRole == UserRole.SuperAdmin)
        {
            return;
        }

        if (currentUserRole is UserRole.Admin &&
            oldUserRole is UserRole.Student or UserRole.Teacher &&
            newUserRole is UserRole.Student or UserRole.Teacher)
        {
            return;
        }

        throw new InvalidOperationException("Недостаточно прав для изменения пользователя с такой ролью");
    }

    /// <summary>
    /// Проверить возможность удалить пользователя с ролью
    /// </summary>
    public static void ValidateDelete(UserRole currentUserRole, UserRole deletedUserRole)
    {
        if (deletedUserRole is UserRole.SuperAdmin)
        {
            throw new InvalidOperationException("Нельзя удалить суперадминистратора через API");
        }

        if (currentUserRole is UserRole.SuperAdmin)
        {
            return;
        }

        if (currentUserRole is UserRole.Admin && deletedUserRole is UserRole.Student or UserRole.Teacher)
        {
            return;
        }

        throw new InvalidOperationException("Недостаточно прав для удаления пользователя с такой ролью");
    }
}