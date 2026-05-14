namespace Security.Auth.Admin.Constants;

/// <summary>
/// Названия ролей для авторизации
/// </summary>
public static class AuthRoleNames
{
    /// <summary>
    /// Роль студента.
    /// </summary>
    public const string Student = "student";

    /// <summary>
    /// Роль преподавателя.
    /// </summary>
    public const string Teacher = "teacher";

    /// <summary>
    /// Роль администратора.
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Роль суперадминистратора.
    /// </summary>
    public const string SuperAdmin = "superadmin";

    /// <summary>
    /// Роли администратора и суперадминистратора.
    /// </summary>
    public const string AdminOrSuperAdmin = $"{Admin},{SuperAdmin}";
}