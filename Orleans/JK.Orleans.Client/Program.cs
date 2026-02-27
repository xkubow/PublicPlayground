using JK.Orleans;
using JK.Orleans.Grains;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Orleans.Configuration;
using Orleans.Hosting;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Host.UseOrleansClient(clientBuilder =>
{
    var connectionString = configuration.GetConnectionString("Orleans");
    var invariant = "Microsoft.Data.SqlClient";

    if (!string.IsNullOrEmpty(connectionString))
    {
        clientBuilder.UseAdoNetClustering(options =>
        {
            options.ConnectionString = connectionString;
            options.Invariant = invariant;
        });
    }
    else
    {
        clientBuilder.UseLocalhostClustering();
    }

    clientBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "dev";
        options.ServiceId = "JK.Orleans";
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" }); });

var app = builder.Build();

app.MapGet("/", static () => "URL Shortener Gateway (Orleans Client) is running.");

app.MapGet("/shorten", static async (IClusterClient client, HttpRequest request, string url) =>
    {
        var host = $"{request.Scheme}://{request.Host.Value}";

        if (string.IsNullOrWhiteSpace(url) ||
            Uri.IsWellFormedUriString(url, UriKind.Absolute) is false)
        {
            return Results.BadRequest($"""
                                       The URL query string is required and needs to be well formed.
                                       Consider, {host}/shorten?url=https://www.microsoft.com.
                                       """);
        }

        var shortenedRouteSegment = Guid.NewGuid().GetHashCode().ToString("X");

        var shortenerGrain = client.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        await shortenerGrain.SetUrl(url);

        var resultBuilder = new UriBuilder(host) { Path = $"/go/{shortenedRouteSegment}" };

        return Results.Ok(resultBuilder.Uri);
    });

app.MapGet("/go/{shortenedRouteSegment:required}", static async (IClusterClient client, string shortenedRouteSegment) =>
    {
        var shortenerGrain = client.GetGrain<IUrlShortenerGrain>(shortenedRouteSegment);
        var url = await shortenerGrain.GetUrl();

        if (string.IsNullOrEmpty(url))
        {
            return Results.NotFound();
        }

        var redirectBuilder = new UriBuilder(url);
        return Results.Redirect(redirectBuilder.Uri.ToString());
    });

app.MapGet("/TestCall", static async (IClusterClient client) =>
    {
        var shortenerGrain = client.GetGrain<IApiMessageGrain>("ApiMessageGrain");
        shortenerGrain.SendApiMessage("grpc://localhost:8001/protos.JK.Platform.Configurations.ConfigurationsService/TestCall");

        return Results.Ok();
    });
app.MapGet("/register", static async (IClusterClient client, string cron) =>
    {
        cron = System.Net.WebUtility.UrlDecode(cron);
        Console.WriteLine($"Received register request with cron: {cron}");
        var grainId = Guid.NewGuid().GetHashCode().ToString("X");
        var apiGrain = client.GetGrain<IApiMessageGrain>(grainId);
        var success = await apiGrain.Register(cron);
        return success ? Results.Ok($"Registered {grainId} with cron: {cron}") : Results.BadRequest("Invalid cron expression");
    });
app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"); });
app.Run();