namespace Infrastructure.Auth.Options;

public sealed class RefreshTokenOptions
{
    public int LifetimeDays { get; init; } = 7;

    public int TokenBytes { get; init; } = 64;
}