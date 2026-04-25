using System.Reflection;
using AutoMapper;
using Infrastructure.AutoMapper.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.AutoMapper;

/// <summary>
/// Настройки AutoMapper
/// </summary>
public static class MapperStartUp
{
    /// <summary>
    /// Добавляет настройки AutoMapper .AddCyberMapper(assemblies: Assembly.GetExecutingAssembly())
    /// </summary>
    public static void AddCyberMapper(this IServiceCollection services, params Assembly[] assemblies)
    {
        // добавляется MapperExtension всегда - не надо волноваться
        var config = new AutoMapperConfiguratonFactory().Create(assemblies);
        config.AssertConfigurationIsValid(); // сразу делаем проверку при старте

        IMapper mapper = new Mapper(config);
        services.AddSingleton(mapper);
    }
}