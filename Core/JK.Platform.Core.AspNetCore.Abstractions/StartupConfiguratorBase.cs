using AutoMapper;
using JK.Platform.Core.Configurators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.AspNetCore.Abstractions;

public abstract class StartupConfiguratorBase<TStartupConfigurator> : ConfiguratorBase<TStartupConfigurator>, IStartupConfigurator
    where TStartupConfigurator : StartupConfiguratorBase<TStartupConfigurator>
{
    public virtual IServiceCollection ConfigureOptions(IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection;

    public virtual IHealthChecksBuilder ConfigureHealthChecks(IHealthChecksBuilder healthChecksBuilder, IConfiguration configuration) => healthChecksBuilder;

    public virtual IServiceCollection ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection;

    public virtual IApplicationBuilder ConfigureExceptionHandlers(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureHttp(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureBeforeStaticFiles(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureStaticFiles(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureCookies(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureBeforeRouting(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureAfterRouting(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureCors(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureAuthentication(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureAuthorization(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureMiddlewares(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IApplicationBuilder ConfigureBeforeEndpoints(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

    public virtual IEndpointRouteBuilder ConfigureEndpoints(IEndpointRouteBuilder endpointRouteBuilder, IApplicationBuilder applicationBuilder, IConfiguration configuration) => endpointRouteBuilder;

    public virtual void ConfigureAutomapperGlobalMappings(IMapperConfigurationExpression mapperConfigurationExpression, IConfiguration configuration)
    { }
}

public abstract class StartupConfiguratorBase<TStartupConfigurator, TChildConfigurator> : ConfiguratorBase<TStartupConfigurator, TChildConfigurator>, IStartupConfigurator
   where TStartupConfigurator : ConfiguratorBase<TStartupConfigurator>
   where TChildConfigurator : IConfiguratorBase
{
   public virtual IServiceCollection ConfigureOptions(IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection;

   public virtual IHealthChecksBuilder ConfigureHealthChecks(IHealthChecksBuilder healthChecksBuilder, IConfiguration configuration) => healthChecksBuilder;

   public virtual IServiceCollection ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration) => serviceCollection;

   public virtual IApplicationBuilder ConfigureExceptionHandlers(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureHttp(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureBeforeStaticFiles(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureStaticFiles(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureCookies(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureCors(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureBeforeRouting(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureAfterRouting(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureAuthentication(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureAuthorization(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureMiddlewares(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IApplicationBuilder ConfigureBeforeEndpoints(IApplicationBuilder applicationBuilder, IConfiguration configuration) => applicationBuilder;

   public virtual IEndpointRouteBuilder ConfigureEndpoints(IEndpointRouteBuilder endpointRouteBuilder, IApplicationBuilder applicationBuilder, IConfiguration configuration) => endpointRouteBuilder;

   public virtual void ConfigureAutomapperGlobalMappings(IMapperConfigurationExpression mapperConfigurationExpression, IConfiguration configuration)
   { }
}