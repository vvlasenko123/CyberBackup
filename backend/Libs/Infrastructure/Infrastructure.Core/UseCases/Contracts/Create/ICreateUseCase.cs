namespace Infrastructure.Core.UseCases.Contracts.Create;

/// <summary>
/// Контракт use case для создания
/// </summary>
public interface ICreateUseCase<in TRequest, TResponse>
{
    /// <summary>
    /// Выполнение операции создания
    /// </summary>
    Task<TResponse> Execute(TRequest request);
}