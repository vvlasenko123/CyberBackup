namespace Infrastructure.Core.Services.Contracts.Create;

/// <summary>
/// Контракт для логики создания Service
/// </summary>
public interface ICreateService<in TRequest>
{
    /// <summary>
    /// Создание без параметров
    /// </summary>
    Task Create(TRequest request, Guid currentUserId, CancellationToken token);
}