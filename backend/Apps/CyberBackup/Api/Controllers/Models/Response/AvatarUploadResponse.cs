namespace Api.Controllers.Models.Response;

/// <summary>
/// Ответ на загрузку аватара
/// </summary>
public sealed record AvatarUploadResponse(DateTimeOffset AvatarUpdatedAt);
