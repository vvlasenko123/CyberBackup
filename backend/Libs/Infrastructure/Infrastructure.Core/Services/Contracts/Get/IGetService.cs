namespace Infrastructure.Core.Services.Contracts.Get;

/// <summary>
/// Контракт для логики получения Service
/// </summary>
public interface IGetService<in TRequest, TResponse>
{
    /// <summary>
    /// Создание без параметров
    /// </summary>
    Task<TResponse> Get(TRequest request);
}