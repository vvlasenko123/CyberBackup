using Domain.Repositories;
using Domain.User;
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
}