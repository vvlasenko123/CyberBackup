using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Core.Controllers.Internal.Filters;

/// <summary>
/// Фильтр, запрещающий доступ к endpoint в production
/// </summary>
public sealed class InternalFilter : IActionFilter
{
    /// <summary>
    /// Окружение приложения
    /// </summary>
    private readonly IHostEnvironment _environment;

    public InternalFilter(IHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Проверка перед выполнением action
    /// </summary>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_environment.IsProduction())
        {
            context.Result = new StatusCodeResult(StatusCodes.Status404NotFound);
        }
    }

    /// <summary>
    /// Выполняется после action
    /// </summary>
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}