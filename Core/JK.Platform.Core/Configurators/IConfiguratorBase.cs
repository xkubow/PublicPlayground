using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace JK.Platform.Core.Configurators;

public interface IConfiguratorBase
{
    void Initialize(IConfiguration configuration);

    void Initialize(ILoggerFactory loggerFactory);
}