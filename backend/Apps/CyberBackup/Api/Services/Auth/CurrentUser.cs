using System.IdentityModel.Tokens.Jwt;
using Application.Abstractions.UseCases.User;
using Application.DTO.Auth;
using Domain.User.Enums;

namespace Api.Services.Auth;

/// <inheritdoc />
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(value, out var userId)
                ? userId
                : Guid.Empty;
        }
    }

    /// <inheritdoc />
    public UserRole Role
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst(AuthClaimNames.Role)?.Value;

            return Enum.TryParse<UserRole>(value, out var role)
                ? role
                : UserRole.Student;
        }
    }
}
