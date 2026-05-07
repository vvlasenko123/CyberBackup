using Domain.Repositories;
using Domain.User;
using Domain.User.Enums;
using Domain.User.ValueObjects;
using Infrastructure.Database.Connection.Contracts;

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
                                   must_change_password, created_by, created_at,updated_at
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
    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken)
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
            new { Email = email },
            cancellationToken);

        return exists;
    }
    
        /// <inheritdoc />
    public async Task<UserModel?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken)
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

        var user = await _connection.QueryFirstOrDefaultAsync<UserRow>(
            sql,
            new { Email = email },
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new UserModel(
            id: user.Id,
            email: new Email(user.Email),
            fullName: new FullName(user.FullName),
            password: new PasswordHash(user.Password),
            role: (UserRole)user.Role,
            isActive: user.IsActive,
            mustChangePassword: user.MustChangePassword,
            createdBy: user.CreatedBy,
            createdAt: user.CreatedAt,
            updatedAt: user.UpdatedAt);
    }

    private sealed class UserRow
    {
        public Guid Id { get; init; }

        public string Email { get; init; }

        public string FullName { get; init; }

        public string Password { get; init; }

        public int Role { get; init; }

        public bool IsActive { get; init; }

        public bool MustChangePassword { get; init; }

        public Guid? CreatedBy { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public DateTimeOffset UpdatedAt { get; init; }
    }
}