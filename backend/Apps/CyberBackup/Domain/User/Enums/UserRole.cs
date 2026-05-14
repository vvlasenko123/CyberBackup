namespace Domain.User.Enums;

/// <summary>
/// Пользовательские роли
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Роль студент
    /// </summary>
    Student = 0,

    /// <summary>
    /// Роль преподаватель
    /// </summary>
    Teacher = 1,

    /// <summary>
    /// Роль администратора
    /// </summary>
    Admin = 2,

    /// <summary>
    /// Роль суперадминистратора
    /// </summary>
    SuperAdmin = 3
}