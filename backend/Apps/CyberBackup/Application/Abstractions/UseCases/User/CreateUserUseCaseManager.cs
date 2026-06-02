using Application.Abstractions.Services.Auth.Contracts;
using Application.Abstractions.Services.User.Contracts;
using Application.Abstractions.UseCases.User.Contracts;
using Application.DTO.User;
using Application.DTO.User.Validate;

namespace Application.Abstractions.UseCases.User;

/// <inheritdoc />
public sealed class CreateUserUseCaseManager : ICreateUserUseCaseManager
{
    private readonly ICreateUserService _createUserService;
    private readonly IJwtService _jwtService;

    public CreateUserUseCaseManager(
        ICreateUserService createUserService,
        IJwtService jwtService)
    {
        _createUserService = createUserService;
        _jwtService = jwtService;
    }

    /// <inheritdoc />
    public async Task<Guid> Execute(UserDto request, CancellationToken cancellationToken)
    {
        var currentUser = _jwtService.GetCurrentUser();

        UserRolePermissionValidator.ValidateCreate(currentUserRole: currentUser.Role, newUserRole: request.Role);

        return await _createUserService.Create(request, currentUser.UserId, cancellationToken);
    }
}