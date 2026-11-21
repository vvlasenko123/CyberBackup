using Application.DTO.Avatars;

namespace Application.Abstractions.Services.Avatars.Contracts;

/// <summary>
/// Загрузка, замена, удаление и чтение аватаров пользователей
/// </summary>
public interface IUserAvatarService
{
    /// <summary>
    /// Загрузить или заменить аватар пользователя, возвращает время загрузки
    /// </summary>
    Task<DateTimeOffset> SetAsync(Guid userId, AvatarUpload? file, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить аватар пользователя. Если аватара нет - ничего не делает
    /// </summary>
    Task RemoveAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Получить аватар пользователя или признак, что клиентская копия актуальна
    /// </summary>
    Task<AvatarDownload> GetAsync(Guid userId, string? ifNoneMatch, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить из хранилища объект, потерявший ссылку из базы. Сбой только логируется
    /// </summary>
    Task DiscardObjectAsync(string? key, CancellationToken cancellationToken);
}
