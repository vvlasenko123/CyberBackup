namespace Application.Abstractions.Services.Avatars;

/// <summary>
/// Ошибка обработки аватара
/// </summary>
public sealed class AvatarException : CodedBadRequestException
{
    public const string FileRequired = "avatar.file_required";
    public const string TooLarge = "avatar.too_large";
    public const string InvalidFormat = "avatar.invalid_format";
    public const string InvalidDimensions = "avatar.invalid_dimensions";
    public const string CorruptedImage = "avatar.corrupted_image";
    public const string AnimationNotAllowed = "avatar.animation_not_allowed";

    public AvatarException(string code, string message)
        : base(code, message)
    {
    }
}
