namespace Application.Abstractions.Services.Avatars.Contracts;

/// <summary>
/// Хранилище аватаров пользователей
/// </summary>
public interface IUserAvatarStorage
{
    /// <summary>
    /// Сохранить объект под указанным ключом
    /// </summary>
    Task SaveAsync(string key, Stream content, long length, string contentType, CancellationToken cancellationToken);

    /// <summary>
    /// Открыть объект на чтение, null - объекта нет в хранилище
    /// </summary>
    Task<Stream?> OpenReadAsync(string key, CancellationToken cancellationToken);

    /// <summary>
    /// Удалить объект
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken);
}
