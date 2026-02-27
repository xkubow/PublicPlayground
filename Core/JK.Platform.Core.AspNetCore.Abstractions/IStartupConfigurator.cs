using AutoMapper;
using JK.Platform.Core.Configurators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.AspNetCore.Abstractions;

public interface IStartupConfigurator: IConfiguratorBase
{
   IServiceCollection ConfigureOptions(IServiceCollection serviceCollection, IConfiguration configuration);

   IHealthChecksBuilder ConfigureHealthChecks(IHealthChecksBuilder healthChecksBuilder, IConfiguration configuration);

   IServiceCollection ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration);

   IApplicationBuilder ConfigureExceptionHandlers(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureHttp(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureBeforeStaticFiles(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureStaticFiles(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureCookies(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureBeforeRouting(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureAfterRouting(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureCors(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureAuthentication(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureAuthorization(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureMiddlewares(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IApplicationBuilder ConfigureBeforeEndpoints(IApplicationBuilder applicationBuilder, IConfiguration configuration);

   IEndpointRouteBuilder ConfigureEndpoints(IEndpointRouteBuilder endpointRouteBuilder, IApplicationBuilder applicationBuilder, IConfiguration configuration);

   void ConfigureAutomapperGlobalMappings(IMapperConfigurationExpression mapperConfigurationExpression, IConfiguration configuration);
}