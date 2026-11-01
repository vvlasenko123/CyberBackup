using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Models.Request;

/// <summary>
/// Запрос на замену контактов в собственном профиле. null или пустая строка очищают значение
/// </summary>
public sealed record UpdateProfileRequest
{
    /// <summary>
    /// Телефон: 10-15 цифр, допустимы пробелы, дефисы, скобки и ведущий +
    /// </summary>
    [MaxLength(200)]
    public string? Phone { get; init; }

    /// <summary>
    /// Telegram: имя пользователя, допускаются @ и ссылка t.me
    /// </summary>
    [MaxLength(200)]
    public string? Telegram { get; init; }

    /// <summary>
    /// VK: короткое имя или идентификатор страницы, допускается ссылка vk.com
    /// </summary>
    [MaxLength(200)]
    public string? Vk { get; init; }
}
