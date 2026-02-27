using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.Configurators;

public class ConfiguratorBase<TConfigurator> : IConfiguratorBase
{
    protected ILogger<TConfigurator>? Logger { get; private set; } = null;
    protected static readonly string ApplicationName = Assembly.GetEntryAssembly()!.GetName().Name ?? Assembly.GetEntryAssembly()!.GetName().FullName;

    protected IConfiguration? Configuration { get; private set; }

    public virtual void Initialize(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public virtual void Initialize(ILoggerFactory loggerFactory)
    {
        Logger ??= loggerFactory.CreateLogger<TConfigurator>();
    }
}

public abstract class ConfiguratorBase<TConfigurator, TChildConfigurator> : ConfiguratorBase<TConfigurator>
    where TConfigurator : ConfiguratorBase<TConfigurator>
    where TChildConfigurator : IConfiguratorBase
{
    protected static readonly IEnumerable<TChildConfigurator> Configurators;

    static ConfiguratorBase()
    {
        Configurators = GetChildConfigurators();
    }

    public override void Initialize(IConfiguration configuration)
    {
        base.Initialize(configuration);
        foreach (var configurator in Configurators)
            configurator.Initialize(configuration);
    }

    public override void Initialize(ILoggerFactory loggerFactory)
    {
        base.Initialize(loggerFactory);
        foreach (var configurator in Configurators)
            configurator.Initialize(loggerFactory);
    }

    private static IEnumerable<TChildConfigurator> GetChildConfigurators()
    {
        var configuratorsTypes = AppDomain.CurrentDomain.GetAppDomainAssemblies().GetImplementationsWithDefaultConstructor<TChildConfigurator>();
        var configurators = new List<TChildConfigurator>();
        foreach (var configuratorType in configuratorsTypes)
            configurators.Add((TChildConfigurator)Activator.CreateInstance(configuratorType)!);
        return configurators;
    }
}