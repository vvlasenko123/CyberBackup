using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Core.Controllers.Public;

/// <summary>
/// Базовый контроллер, доступный только вне production
/// </summary>
[Route("")]
public class PublicController : ControllerBase
{
    
}
