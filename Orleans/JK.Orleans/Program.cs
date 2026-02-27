using System.Net;
using JK.Orleans;
using JK.Platform.Api.Grpc.Client;
using JK.Platform.Core.Configuration;
using JK.Platform.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Configuration;
using Grpc.Net.Client.Balancer;

var builder = Host.CreateApplicationBuilder(args);
var configuration = builder.Configuration;

builder.Services.ConfigureConfiguration<GrpcClientConfiguration>(configuration);
builder.Services.RegisterInjectableServices();
builder.Services.AddSingleton<ResolverFactory>(sp => new DnsResolverFactory(refreshInterval: TimeSpan.FromSeconds(5)));
builder.Services.AddHttpClient();

builder.UseOrleans(siloBuilder =>
{
    var connectionString = configuration.GetConnectionString("Orleans");
    var invariant = "Microsoft.Data.SqlClient";

    if (!string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine($"[DEBUG] Silo connecting to: {connectionString}");
        siloBuilder.UseAdoNetClustering(options =>
        {
            options.ConnectionString = connectionString;
            options.Invariant = invariant;
        });

        siloBuilder.AddAdoNetGrainStorage("orleans", options =>
        {
            options.ConnectionString = connectionString;
            options.Invariant = invariant;
            options.GrainStorageSerializer = new GrainStorageJsonSerializer();
        });

        siloBuilder.UseAdoNetReminderService(options =>
        {
            options.ConnectionString = connectionString;
            options.Invariant = invariant;
        });
    }
    else
    {
        // Fallback for development if no connection string is provided
        siloBuilder.UseLocalhostClustering();
        siloBuilder.AddMemoryGrainStorage("orleans");
    }

    siloBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "dev";
        options.ServiceId = "JK.Orleans";
    });

    siloBuilder.ConfigureEndpoints(IPAddress.Loopback, siloPort: 11111, gatewayPort: 30000);

    siloBuilder.UseDashboard(options => { options.HostSelf = true; });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Logging.AddFilter("Orleans.Runtime", LogLevel.Debug);
builder.Logging.AddFilter("Orleans.Messaging", LogLevel.Debug);
builder.Logging.AddFilter("Orleans.Clustering.AdoNet", LogLevel.Debug);

using var host = builder.Build();
await host.RunAsync();