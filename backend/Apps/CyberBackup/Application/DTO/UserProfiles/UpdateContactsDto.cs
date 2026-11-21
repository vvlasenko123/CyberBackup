namespace Application.DTO.UserProfiles;

/// <summary>
/// Контакты пользователя. null или пустая строка очищают значение
/// </summary>
public sealed record UpdateContactsDto
{
    public string? Phone { get; init; }

    public string? Telegram { get; init; }

    public string? Vk { get; init; }
}
