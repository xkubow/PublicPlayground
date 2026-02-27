using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Core.Configurators;

public interface IHostConfigurator
{
    void ApplicationStarting(IConfigurationRoot configuration);
    IConfigurationBuilder ConfigureHostConfiguration(IConfigurationBuilder builder);
    IConfigurationBuilder ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder builder);
    void ApplicationBeforeRun(IConfigurationRoot configuration);
    void ApplicationStoppedWithException(Exception exception);
    void ApplicationStopped();
}