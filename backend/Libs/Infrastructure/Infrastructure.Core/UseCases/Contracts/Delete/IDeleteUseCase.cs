namespace Infrastructure.Core.UseCases.Contracts.Delete;

/// <summary>
/// Контракт use case для удаления
/// </summary>
public interface IDeleteUseCase<in TRequest, TResponse>
{
    /// <summary>
    /// Выполнение операции удаления
    /// </summary>
    Task<TResponse> Execute(TRequest request);
}