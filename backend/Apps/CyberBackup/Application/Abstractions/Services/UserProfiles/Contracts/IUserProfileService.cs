using Application.DTO.UserProfiles;
using Domain.User;

namespace Application.Abstractions.Services.UserProfiles.Contracts;

/// <summary>
/// Чтение и обновление собственного профиля
/// </summary>
public interface IUserProfileService
{
    /// <summary>
    /// Получить профиль пользователя
    /// </summary>
    Task<UserModel> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Полностью заменить контакты пользователя и вернуть обновлённый профиль
    /// </summary>
    Task<UserModel> UpdateContactsAsync(Guid userId, UpdateContactsDto request, CancellationToken cancellationToken);
}
