using Security.Auth.Admin.Constants;

namespace Api.Auth;

/// <summary>
/// Роли для атрибутов авторизации
/// </summary>
public static class AuthRoles
{
    public const string Student = AuthRoleNames.Student;
    public const string Teacher = AuthRoleNames.Teacher;
    public const string Admin = AuthRoleNames.Admin;
    public const string SuperAdmin = AuthRoleNames.SuperAdmin;
    public const string StudentTeacherAdmin = Student + "," + Teacher + "," + Admin + "," + SuperAdmin;
    public const string TeacherAdmin = Teacher + "," + Admin + "," + SuperAdmin;
}
