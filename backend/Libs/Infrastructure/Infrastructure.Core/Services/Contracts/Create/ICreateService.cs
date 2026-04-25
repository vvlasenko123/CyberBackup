namespace Infrastructure.Core.Services.Contracts.Create;

/// <summary>
/// Контракт для логики создания Service
/// </summary>
public interface ICreateService<in TRequest, TResponse>
{
    /// <summary>
    /// Создание без параметров
    /// </summary>
    Task<TResponse> Create(TRequest request);
}