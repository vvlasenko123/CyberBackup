namespace Domain.User;

/// <summary>
/// Состояние аватара существующего пользователя
/// </summary>
/// <param name="Path">Ключ объекта в хранилище, null - аватара нет</param>
/// <param name="UpdatedAt">Время последней успешной загрузки</param>
public sealed record UserAvatarState(string? Path, DateTimeOffset? UpdatedAt);

/// <summary>
/// Результат изменения аватара в базе
/// </summary>
/// <param name="UserFound">Найдена ли строка пользователя</param>
/// <param name="PreviousPath">Ключ, который был записан до изменения (вытесненный объект)</param>
public sealed record AvatarChangeResult(bool UserFound, string? PreviousPath);

/// <summary>
/// Исход записи аватара в базу неизвестен (например, оборвалось соединение во время коммита)
/// </summary>
public sealed class AvatarWriteOutcomeUnknownException : Exception
{
    public AvatarWriteOutcomeUnknownException(Exception inner)
        : base("Исход записи аватара в базу данных неизвестен", inner)
    {
    }
}
