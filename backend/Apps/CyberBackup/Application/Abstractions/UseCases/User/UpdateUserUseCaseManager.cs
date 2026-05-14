using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.User.Contracts;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO.User;
using Application.DTO.User.Validate;

namespace Application.Abstractions.UseCases.User;

/// <inheritdoc />
public sealed class UpdateUserUseCaseManager : IUpdateUserUseCaseManager
{
    private readonly IUpdateUserService _updateUserService;
    private readonly IJwtService _jwtService;

    public UpdateUserUseCaseManager(
        IUpdateUserService updateUserService,
        IJwtService jwtService)
    {
        _updateUserService = updateUserService;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public async Task Execute(UpdateUserDto request, CancellationToken cancellationToken)
    {
        var currentUser = _jwtService.GetCurrentUser();
        var user = await _updateUserService.GetForUpdate(request.Id, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("Пользователь не найден");
        }

        UserRolePermissionValidator.ValidateUpdate(
            currentUserRole: currentUser.Role,
            oldUserRole: user.Role,
            newUserRole: request.Role);

        await _updateUserService.Update(request, cancellationToken);
    }
}