namespace Infrastructure.Database.Base.Constants;

/// <summary>
/// Константы базы данных
/// </summary>
public static class DbConst
{
    /// <summary>
    /// Базы данных
    /// </summary>
    public static class Database
    {
        /// <summary>
        /// Системная база данных PostgreSQL
        /// </summary>
        public const string Postgres = "postgres";
    }
    
    /// <summary>
    /// Названия таблиц
    /// </summary>
    public static class Tables
    {
        /// <summary>
        /// Таблица пользователей
        /// </summary>
        public const string Users = "users";
        
        /// <summary>
        /// Таблица групп
        /// </summary>
        public const string Groups = "groups";
        
        /// <summary>
        /// Таблица связи пользователей и групп
        /// </summary>
        public const string UserGroups = "user_groups";
    }
    
    /// <summary>
    /// Названия колонок
    /// </summary>
    public static class Columns
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public const string Id = "id";

        /// <summary>
        /// Почта
        /// </summary>
        public const string Email = "email";

        /// <summary>
        /// Хэш пароля
        /// </summary>
        public const string PasswordHash = "password_hash";

        /// <summary>
        /// Полное имя
        /// </summary>
        public const string FullName = "full_name";

        /// <summary>
        /// Роль
        /// </summary>
        public const string Role = "role";

        /// <summary>
        /// Активность
        /// </summary>
        public const string IsActive = "is_active";

        /// <summary>
        /// Требуется смена пароля
        /// </summary>
        public const string MustChangePassword = "must_change_password";

        /// <summary>
        /// Кто создал
        /// </summary>
        public const string CreatedBy = "created_by";

        /// <summary>
        /// Дата создания
        /// </summary>
        public const string CreatedAt = "created_at";

        /// <summary>
        /// Дата обновления
        /// </summary>
        public const string UpdatedAt = "updated_at";

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public const string UserId = "user_id";
        
        /// <summary>
        /// Идентификатор группы
        /// </summary>
        public const string GroupId = "group_id";

        /// <summary>
        /// Название
        /// </summary>
        public const string Name = "name";
    }
}