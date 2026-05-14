namespace Infrastructure.Core.Services.Contracts.Delete;

/// <summary>
/// Контракт для логики удаления Service
/// </summary>
public interface IDeleteService<in TRequest>
{
    /// <summary>
    /// удаление без параметров
    /// </summary>
    Task Delete(TRequest request, CancellationToken cancellationToken);
}