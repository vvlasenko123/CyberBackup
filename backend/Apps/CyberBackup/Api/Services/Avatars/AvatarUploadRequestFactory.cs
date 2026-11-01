using Application.DTO.Avatars;
using Microsoft.AspNetCore.Http;

namespace Api.Services.Avatars;

/// <inheritdoc />
public sealed class AvatarUploadRequestFactory : IAvatarUploadRequestFactory
{
    /// <inheritdoc />
    public AvatarUpload? Create(IFormFile? file)
    {
        if (file is null)
        {
            return null;
        }

        return new AvatarUpload(file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
    }
}
