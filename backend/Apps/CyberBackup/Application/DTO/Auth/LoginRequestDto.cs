namespace Application.DTO.Auth;

/// <summary>
/// DTO входа.
/// </summary>
public sealed record LoginRequestDto(
    string Email,
    string Password);