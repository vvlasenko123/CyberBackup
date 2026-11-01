using Application.Abstractions.Services.UserProfiles.Contracts;
using Application.DTO.UserProfiles;
using Domain.Repositories;
using Domain.User;

namespace Application.Abstractions.Services.UserProfiles;

/// <inheritdoc />
public sealed class UserProfileService : IUserProfileService
{
    private readonly IUserRepository _userRepository;

    public UserProfileService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    public async Task<UserModel> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        return user ?? throw new KeyNotFoundException("Пользователь не найден");
    }

    /// <inheritdoc />
    public async Task<UserModel> UpdateContactsAsync(
        Guid userId,
        UpdateContactsDto request,
        CancellationToken cancellationToken)
    {
        var phone = ContactNormalizer.NormalizePhone(request.Phone);
        var telegram = ContactNormalizer.NormalizeTelegram(request.Telegram);
        var vk = ContactNormalizer.NormalizeVk(request.Vk);

        var updated = await _userRepository.UpdateContactsAsync(
            userId,
            phone,
            telegram,
            vk,
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (!updated)
        {
            throw new KeyNotFoundException("Пользователь не найден");
        }

        return await GetAsync(userId, cancellationToken);
    }
}
