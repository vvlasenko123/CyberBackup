using Domain.Repositories;
using Domain.User;
using Infrastructure.Database.Connection.Contracts;

namespace Infrastructure.Database.Users;

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
}