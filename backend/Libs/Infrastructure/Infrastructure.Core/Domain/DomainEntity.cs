namespace Infrastructure.Core.Domain;

/// <summary>
/// Базовая доменная сущность с айди
/// </summary>
public abstract class DomainEntity
{
    /// <summary>
    /// айди
    /// </summary>
    public Guid Id { get; protected set; }
}