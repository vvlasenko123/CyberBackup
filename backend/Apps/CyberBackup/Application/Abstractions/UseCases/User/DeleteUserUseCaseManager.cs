using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.User.Contracts;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO.User.Validate;

namespace Application.Abstractions.UseCases.User;

/// <inheritdoc />
public sealed class DeleteUserUseCaseManager : IDeleteUserUseCaseManager
{
    private readonly IDeleteUserService _deleteUserService;
    private readonly IJwtService _jwtService;

    public DeleteUserUseCaseManager(
        IDeleteUserService deleteUserService,
        IJwtService jwtService)
    {
        _deleteUserService = deleteUserService;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public async Task Execute(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = _jwtService.GetCurrentUser();
        var user = await _deleteUserService.GetForDelete(id, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Пользователь не найден");
        }

        if (currentUser.UserId == id)
        {
            throw new InvalidOperationException("Нельзя удалить самого себя");
        }

        UserRolePermissionValidator.ValidateDelete(currentUserRole: currentUser.Role, deletedUserRole: user.Role);
        await _deleteUserService.Delete(id, cancellationToken);
    }
}