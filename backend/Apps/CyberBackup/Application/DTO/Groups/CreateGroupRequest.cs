namespace Application.DTO.Groups;

/// <summary>
/// Запрос создания группы
/// </summary>
public sealed record CreateGroupRequest
{
    public string Name { get; init; } = string.Empty;
}
