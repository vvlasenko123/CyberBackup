using Application.Abstractions.Services.Avatars.Contracts;
using Application.Abstractions.Services.User.Contracts;
using Domain.Repositories;
using Domain.User;

namespace Application.Abstractions.Services.User;

/// <inheritdoc />
public sealed class DeleteUserService : IDeleteUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserAvatarService _avatarService;

    public DeleteUserService(IUserRepository userRepository, IUserAvatarService avatarService)
    {
        _userRepository = userRepository;
        _avatarService = avatarService;
    }

    /// <inheritdoc />
    public async Task<UserModel?> GetForDelete(Guid id, CancellationToken cancellationToken)
    {
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        // Ключ аватара приходит из самой операции удаления, поэтому гонки с параллельной загрузкой нет
        var avatarPath = await _userRepository.DeleteUserAsync(id, cancellationToken);

        await _avatarService.DiscardObjectAsync(avatarPath, CancellationToken.None);
    }
}