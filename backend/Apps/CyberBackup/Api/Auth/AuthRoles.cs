using Domain.User.Enums;

namespace Api.Auth;

/// <summary>
/// Роли для атрибутов авторизации
/// </summary>
public static class AuthRoles
{
    public const string Student = nameof(UserRole.Student);
    public const string Teacher = nameof(UserRole.Teacher);
    public const string Admin = nameof(UserRole.Admin);
    public const string StudentTeacherAdmin = Student + "," + Teacher + "," + Admin;
    public const string TeacherAdmin = Teacher + "," + Admin;
}
