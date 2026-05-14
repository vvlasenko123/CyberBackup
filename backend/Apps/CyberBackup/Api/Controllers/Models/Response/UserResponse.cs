using Domain.User.Enums;

namespace Api.Controllers.Models.Response;

/// <summary>
/// Ответ с пользователем
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive,
    bool MustChangePassword,
    Guid? CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);