using Domain.Repositories;
using Domain.User;
using Domain.User.Enums;
using Domain.User.ValueObjects;
using Infrastructure.Database.Connection.Contracts;
using Infrastructure.Repositories.Models;

namespace Infrastructure.Repositories;

/// <inheritdoc />
public sealed class UserRepository : IUserRepository
{
    private readonly IAsyncDbConnection _connection;

    public UserRepository(IAsyncDbConnection connection)
    {
        _connection = connection;
    }

    /// <inheritdoc />
    public async Task CreateUserAsync(UserModel userModel, CancellationToken cancellationToken)
    {
        const string sql = """
                               INSERT INTO users (
                                   id, email, full_name, password, role, is_active,
                                   must_change_password, created_by, created_at, updated_at
                               )
                               VALUES (
                                   @Id, @Email, @FullName, @Password, @Role, @IsActive,
                                   @MustChangePassword, @CreatedBy, @CreatedAt, @UpdatedAt
                               )
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            userModel.Id,
            Email = userModel.Email.Value,
            FullName = userModel.FullName.Value,
            Password = userModel.Password.Value,
            Role = (int) userModel.Role,
            userModel.IsActive,
            userModel.MustChangePassword,
            userModel.CreatedBy,
            userModel.CreatedAt,
            userModel.UpdatedAt
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task UpdateUserAsync(UserModel userModel, CancellationToken cancellationToken)
    {
        const string sql = """
                               UPDATE users
                               SET
                                   email = @Email,
                                   full_name = @FullName,
                                   password = @Password,
                                   role = @Role,
                                   is_active = @IsActive,
                                   must_change_password = @MustChangePassword,
                                   updated_at = @UpdatedAt
                               WHERE id = @Id
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            userModel.Id,
            Email = userModel.Email.Value,
            FullName = userModel.FullName.Value,
            Password = userModel.Password.Value,
            Role = (int) userModel.Role,
            userModel.IsActive,
            userModel.MustChangePassword,
            userModel.UpdatedAt
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
                               DELETE FROM users
                               WHERE id = @Id
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            Id = id
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id AS Id,
                                   email AS Email,
                                   full_name AS FullName,
                                   password AS Password,
                                   role AS Role,
                                   is_active AS IsActive,
                                   must_change_password AS MustChangePassword,
                                   created_by AS CreatedBy,
                                   created_at AS CreatedAt,
                                   updated_at AS UpdatedAt
                               FROM users
                               WHERE id = @Id
                               LIMIT 1;
                           """;

        var user = await _connection.QueryFirstOrDefaultAsync<UserDbModel>(
            sql,
            new
            {
                Id = id
            },
            cancellationToken);

        return MapToModel(user);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<UserModel>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id AS Id,
                                   email AS Email,
                                   full_name AS FullName,
                                   password AS Password,
                                   role AS Role,
                                   is_active AS IsActive,
                                   must_change_password AS MustChangePassword,
                                   created_by AS CreatedBy,
                                   created_at AS CreatedAt,
                                   updated_at AS UpdatedAt
                               FROM users
                               ORDER BY created_at DESC;
                           """;

        var users = await _connection.QueryAsync<UserDbModel>(
            sql,
            null,
            cancellationToken);

        return users
            .Select(MapToModel)
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT EXISTS (
                                   SELECT 1
                                   FROM users
                                   WHERE LOWER(email) = LOWER(@Email)
                               );
                           """;

        var exists = await _connection.QueryFirstOrDefaultAsync<bool>(
            sql,
            new
            {
                Email = email
            },
            cancellationToken);

        return exists;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailForAnotherUserAsync(Guid userId, string email, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT EXISTS (
                                   SELECT 1
                                   FROM users
                                   WHERE LOWER(email) = LOWER(@Email)
                                     AND id <> @UserId
                               );
                           """;

        var exists = await _connection.QueryFirstOrDefaultAsync<bool>(
            sql,
            new
            {
                UserId = userId,
                Email = email
            },
            cancellationToken);

        return exists;
    }

    /// <inheritdoc />
    public async Task<UserModel?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT
                                   id AS Id,
                                   email AS Email,
                                   full_name AS FullName,
                                   password AS Password,
                                   role AS Role,
                                   is_active AS IsActive,
                                   must_change_password AS MustChangePassword,
                                   created_by AS CreatedBy,
                                   created_at AS CreatedAt,
                                   updated_at AS UpdatedAt
                               FROM users
                               WHERE LOWER(email) = LOWER(@Email)
                               LIMIT 1;
                           """;

        var user = await _connection.QueryFirstOrDefaultAsync<UserDbModel>(
            sql,
            new
            {
                Email = email
            },
            cancellationToken);

        return MapToModel(user);
    }

    /// <inheritdoc />
    public async Task ChangePasswordAsync(Guid userId, PasswordHash passwordHash, CancellationToken cancellationToken)
    {
        const string sql = """
                               UPDATE users
                               SET
                                   password = @Password,
                                   must_change_password = false,
                                   updated_at = @UpdatedAt
                               WHERE id = @UserId
                           """;

        await _connection.ExecuteAsync(sql, new
        {
            UserId = userId,
            Password = passwordHash.Value,
            UpdatedAt = DateTimeOffset.UtcNow
        }, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Guid>> GetStudentIdsByTeacherAsync(Guid teacherId, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT DISTINCT ug.user_id AS "Id"
                               FROM teacher_groups tg
                               JOIN user_groups ug ON ug.group_id = tg.group_id
                               JOIN users u ON u.id = ug.user_id AND u.role = @StudentRole
                               WHERE tg.teacher_id = @TeacherId;
                           """;

        var result = await _connection.QueryAsync<GuidDbModel>(
            sql,
            new { TeacherId = teacherId, StudentRole = (int)UserRole.Student },
            cancellationToken);
        return result.Select(x => x.Id).ToList();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Guid>> GetUserIdsByRolesAsync(IEnumerable<int> roles, CancellationToken cancellationToken)
    {
        const string sql = """
                               SELECT id AS "Id" FROM users WHERE role = ANY(@Roles);
                           """;

        var result = await _connection.QueryAsync<GuidDbModel>(sql, new { Roles = roles.ToArray() }, cancellationToken);
        return result.Select(x => x.Id).ToList();
    }

    private sealed class GuidDbModel
    {
        public Guid Id { get; init; }
    }

    /// <summary>
    /// Преобразовать db model в domain model
    /// </summary>
    private static UserModel? MapToModel(UserDbModel? user)
    {
        if (user is null)
        {
            return null;
        }

        return new UserModel(
            id: user.Id,
            email: new Email(user.Email),
            fullName: new FullName(user.FullName),
            password: new PasswordHash(user.Password),
            role: (UserRole) user.Role,
            isActive: user.IsActive,
            mustChangePassword: user.MustChangePassword,
            createdBy: user.CreatedBy,
            createdAt: user.CreatedAt,
            updatedAt: user.UpdatedAt);
    }
}