using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.Configurators;

public class HostConfiguratorBase: IHostConfigurator
{
    public void ApplicationStarting(IConfigurationRoot configuration)
    {
    }

    public IConfigurationBuilder ConfigureHostConfiguration(IConfigurationBuilder builder) => builder;

    public virtual IConfigurationBuilder ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder configurationBuilder) => configurationBuilder;
    public void ApplicationBeforeRun(IConfigurationRoot configuration)
    {
    }

    public void ApplicationStoppedWithException(Exception exception)
    {
    }

    public void ApplicationStopped()
    {
    }

    public virtual IConfigurationBuilder ConfigureConfiguration(IConfigurationBuilder configurationBuilder, string[] args) => configurationBuilder;
}

public abstract class HostConfiguratorBase<TChildConfigurator> : IHostConfigurator
{
    protected static readonly IEnumerable<TChildConfigurator> Configurators;

    static HostConfiguratorBase()
    {
        Configurators = GetChildConfigurators();
    }

    public virtual IConfigurationBuilder ConfigureConfiguration(IConfigurationBuilder configurationBuilder, string[] args) => configurationBuilder;

    public virtual IConfigurationRoot UpdateConfiguration(IConfigurationRoot configurationRoot) => configurationRoot;

    public virtual IConfigurationBuilder ConfigureHostConfiguration(IConfigurationBuilder configurationBuilder) => configurationBuilder;

    public virtual IConfigurationBuilder ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder configurationBuilder) => configurationBuilder;

    public virtual ILoggingBuilder ConfigureLogging(HostBuilderContext hostContext, ILoggingBuilder loggingBuilder) => loggingBuilder;

    public virtual void ApplicationStarting(IConfigurationRoot configurationRoot)
    { }

    public virtual void ApplicationBeforeRun(IConfigurationRoot configurationRoot)
    { }

    public virtual void ApplicationStopped()
    { }

    public virtual void ApplicationStoppedWithException(Exception exception)
    { }

    private static IEnumerable<TChildConfigurator> GetChildConfigurators()
    {
        var configuratorsTypes = AppDomain.CurrentDomain.GetAppDomainAssemblies().GetImplementationsWithDefaultConstructor<TChildConfigurator>();
        var configurators = new List<TChildConfigurator>();
        foreach (var configuratorType in configuratorsTypes)
            configurators.Add((TChildConfigurator)Activator.CreateInstance(configuratorType)!);
        return configurators;
    }
}