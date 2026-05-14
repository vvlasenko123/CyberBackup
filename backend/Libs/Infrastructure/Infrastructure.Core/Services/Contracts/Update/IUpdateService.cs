namespace Infrastructure.Core.Services.Contracts.Update;

/// <summary>
/// Контракт для логики обновления Service
/// </summary>
public interface IUpdateService<in TRequest>
{
    /// <summary>
    /// Обновление без параметров
    /// </summary>
    Task Update(TRequest request, CancellationToken cancellationToken);
}