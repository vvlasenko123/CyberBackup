namespace Infrastructure.Core.UseCases.Contracts.Get;

/// <summary>
/// Контракт use case для получения
/// </summary>
public interface IGetUseCase<in TRequest, TResponse>
{
    /// <summary>
    /// Выполнение операции получения
    /// </summary>
    Task<TResponse?> Execute(TRequest request, CancellationToken cancellationToken);
}