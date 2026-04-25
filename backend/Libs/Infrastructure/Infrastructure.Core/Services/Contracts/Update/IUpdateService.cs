namespace Infrastructure.Core.Services.Contracts.Update;

/// <summary>
/// Контракт для логики обновления Service
/// </summary>
public interface IUpdateService<in TRequest, TResponse>
{
    /// <summary>
    /// Создание без параметров
    /// </summary>
    Task<TResponse> Update(TRequest request);
}