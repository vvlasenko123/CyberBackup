namespace Infrastructure.Core.UseCases.Contracts.Create;

/// <summary>
/// Контракт use case для создания
/// </summary>
public interface ICreateUseCase<in TRequest>
{
    /// <summary>
    /// Выполнение операции создания
    /// </summary>
    Task Execute(TRequest request, CancellationToken cancellationToken);
}