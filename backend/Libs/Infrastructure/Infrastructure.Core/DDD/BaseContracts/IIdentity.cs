namespace Infrastructure.Core.DDD.BaseContracts;

/// <summary>
/// Базовый контракт с айди
/// </summary>
public interface IIdentity<out TType>
{
    /// <summary>
    /// айди
    /// </summary>
    TType Id { get; }
}
