namespace Infrastructure.Core.UseCases.Contracts.Delete;

/// <summary>
/// Контракт use case для удаления
/// </summary>
public interface IDeleteUseCase<in TRequest>
{
    /// <summary>
    /// Выполнение операции удаления
    /// </summary>
    Task Execute(TRequest request, CancellationToken cancellationToken);
}