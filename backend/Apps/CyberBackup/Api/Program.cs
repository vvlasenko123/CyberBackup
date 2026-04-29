using Infrastructure.Hosting.HostConfiguration;

namespace Api;

/// <summary>
/// Класс определяющий точку входа
/// </summary>
public abstract class Program
{
    /// <summary>
    /// Старт приложения
    /// </summary>
    public static void Main(string[] args)
    {
        HostFactory.CreateHostBuilder<StartUp>(args).Build().Run();
    }
}