namespace Infrastructure.Core.Services.Contracts.Delete;

/// <summary>
/// Контракт для логики удаления Service
/// </summary>
public interface IDeleteService<in TRequest, TResponse>
{
    /// <summary>
    /// Создание без параметров
    /// </summary>
    Task<TResponse> Delete(TRequest request);
}