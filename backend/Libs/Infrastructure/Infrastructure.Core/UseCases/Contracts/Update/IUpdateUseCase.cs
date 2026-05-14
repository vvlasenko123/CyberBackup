namespace Infrastructure.Core.UseCases.Contracts.Update;

/// <summary>
/// Контракт use case для обновления
/// </summary>
public interface IUpdateUseCase<in TRequest>
{
    /// <summary>
    /// Выполнение операции обновления
    /// </summary>
    Task Execute(TRequest request, CancellationToken cancellationToken);
}