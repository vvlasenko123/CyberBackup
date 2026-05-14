using Application.Abstractions.Services.User.Contracts;
using Domain.Repositories;
using Domain.User;

namespace Application.Abstractions.Services.User;

/// <inheritdoc />
public sealed class DeleteUserService : IDeleteUserService
{
    private readonly IUserRepository _userRepository;

    public DeleteUserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<UserModel?> GetForDelete(Guid id, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await _userRepository.DeleteUserAsync(id, cancellationToken);
    }
}