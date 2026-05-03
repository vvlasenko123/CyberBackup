namespace Application.DTO.Auth;

/// <summary>
/// DTO регистрации.
/// </summary>
public sealed record RegisterRequestDto(
    string Email,
    string FullName,
    string Password);