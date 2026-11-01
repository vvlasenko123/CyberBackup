namespace Application.Abstractions.Services.UserProfiles;

/// <summary>
/// Ошибка данных профиля
/// </summary>
public sealed class ProfileException : CodedBadRequestException
{
    public const string PhoneInvalid = "profile.phone_invalid";
    public const string TelegramInvalid = "profile.telegram_invalid";
    public const string VkInvalid = "profile.vk_invalid";

    public ProfileException(string code, string message)
        : base(code, message)
    {
    }
}
