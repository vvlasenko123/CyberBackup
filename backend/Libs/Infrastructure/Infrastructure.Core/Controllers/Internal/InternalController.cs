using Infrastructure.Core.Controllers.Internal.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Core.Controllers.Internal;

/// <summary>
/// Базовый контроллер, доступный только вне production
/// </summary>
[ServiceFilter(typeof(InternalFilter))]
[Route("")]
public abstract class InternalController : ControllerBase
{
}