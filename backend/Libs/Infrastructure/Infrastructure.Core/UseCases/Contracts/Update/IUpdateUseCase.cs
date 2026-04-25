namespace Infrastructure.Core.UseCases.Contracts.Update;

/// <summary>
/// Контракт use case для обновления
/// </summary>
public interface IUpdateUseCase<in TRequest, TResponse>
{
    /// <summary>
    /// Выполнение операции обновления
    /// </summary>
    Task<TResponse> Execute(TRequest request);
}