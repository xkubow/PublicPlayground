using JK.Platform.Core.AspNetCore.Abstractions.Kestrel;
using JK.Platform.Core.Configuration;
using JK.Platform.Core.Configurators;
using Microsoft.Extensions.Configuration;

namespace JK.Platform.Core.AspNetCore.Abstractions;

public class WebHostConfiguratorBase: HostConfiguratorBase, IWebHostConfigurator
{
    public virtual void ConfigureKestrelOptions(KestrelOptions options, IConfigurationRoot configuration)
    {
    }

    protected void ApplyWebHostConfiguration<T>(KestrelOptions options, IConfigurationRoot configuration) where T : WebHostConfiguration, new()
    {
        WebHostConfiguratorBaseInternal.ApplyWebHostConfigurationInternal<T>(options, configuration);
    }
}

public abstract class WebHostConfiguratorBase<TChildConfigurator> : HostConfiguratorBase<TChildConfigurator>, IWebHostConfigurator
{
    public virtual void ConfigureKestrelOptions(KestrelOptions options, IConfigurationRoot configuration)
    {
    }

    protected void ApplyWebHostConfiguration<T>(KestrelOptions options, IConfigurationRoot configuration) where T : WebHostConfiguration, new()
    {
        WebHostConfiguratorBaseInternal.ApplyWebHostConfigurationInternal<T>(options, configuration);
    }
}

internal static class WebHostConfiguratorBaseInternal
{
    internal static void ApplyWebHostConfigurationInternal<T>(KestrelOptions options, IConfigurationRoot configuration) where T : WebHostConfiguration, new()
    {
        var webHostConfig = configuration.GetConfiguration<T>();

        var port = webHostConfig.GetPort();
        // if (options.PortAlreadyUsed(port))
        //     if (options.GetProtocolsForPort(port) != webHostConfig.Protocols)
        //         throw new ArgumentException($"Port {port} already used for Kestrel in the same process with different protocols set");
        //     else
        //         return;

        switch (webHostConfig.ListenType)
        {
            case KestrelListenIPType.Any:
                options.ListenAnyIp(port, webHostConfig.Protocols, webHostConfig.UseHttps);
                break;

            case KestrelListenIPType.Specific:
                if (webHostConfig.IpAddress == null)
                    throw new ArgumentException($"IP Address must not be null for type Specific");

                options.Listen(webHostConfig.IpAddress!, port, webHostConfig.Protocols, webHostConfig.UseHttps);
                break;

            case KestrelListenIPType.Localhost:
                options.ListenLocalhost(port, webHostConfig.Protocols, webHostConfig.UseHttps);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}