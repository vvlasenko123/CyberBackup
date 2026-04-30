using Infrastructure.Core.Controllers.Internal;
using Infrastructure.Core.Controllers.Public;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Infrastructure.Core.Controllers.Convention;

/// <summary>
/// Конвенция для добавления префикса маршрута в зависимости от базового контроллера
/// </summary>
public sealed class BaseControllerRouteConvention : IControllerModelConvention
{
    /// <summary>
    /// Применение конвенции к контроллеру
    /// </summary>
    public void Apply(ControllerModel controller)
    {
        if (typeof(InternalController).IsAssignableFrom(controller.ControllerType))
        {
            ApplyPrefix(controller, "internal");
            return;
        }

        if (typeof(PublicController).IsAssignableFrom(controller.ControllerType))
        {
            ApplyPrefix(controller, "public");
        }
    }

    /// <summary>
    /// Добавление префикса к маршрутам контроллера
    /// </summary>
    private static void ApplyPrefix(ControllerModel controller, string prefix)
    {
        foreach (var selector in controller.Selectors)
        {
            if (selector.AttributeRouteModel is null)
            {
                continue;
            }

            selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                new AttributeRouteModel
                {
                    Template = prefix
                },
                selector.AttributeRouteModel);
        }
    }
}