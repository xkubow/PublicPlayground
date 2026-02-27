using Grpc.Net.Client.Balancer;
using JK.Platform.Core.AspNetCore.Abstractions;
using JK.Platform.Core.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Api.Grpc.Client;

public class StartupConfigurator : StartupConfiguratorBase<StartupConfigurator>
{
   public override IServiceCollection ConfigureOptions(IServiceCollection serviceCollection, IConfiguration configuration)
      => serviceCollection.ConfigureConfiguration<GrpcClientConfiguration>(configuration);

   public override IServiceCollection ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
   {
      serviceCollection.AddSingleton<ResolverFactory>(sp => new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(5)));
      serviceCollection.AddHttpClient();

      return serviceCollection;
   }
}