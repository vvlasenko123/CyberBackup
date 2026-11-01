using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Filters;

/// <summary>
/// Отключает чтение формы провайдерами значений MVC. Иначе превышение лимита тела запроса
/// превращается в ошибку модели (400) ещё до действия, а должно оставаться 413 от Kestrel.
/// Действие читает форму само через <see cref="HttpRequest.ReadFormAsync(CancellationToken)"/>
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class DisableFormValueProviderAttribute : Attribute, IResourceFilter
{
    /// <inheritdoc />
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        context.ValueProviderFactories.RemoveType<FormValueProviderFactory>();
        context.ValueProviderFactories.RemoveType<FormFileValueProviderFactory>();
        context.ValueProviderFactories.RemoveType<JQueryFormValueProviderFactory>();
    }

    /// <inheritdoc />
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
