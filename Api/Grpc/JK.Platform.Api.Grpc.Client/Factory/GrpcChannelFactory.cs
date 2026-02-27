using System.Diagnostics;
using System.Text.Json;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JK.Platform.Api.Grpc.Client.Factory;

[Injectable(lifetime: ServiceLifetime.Singleton)]
public class GrpcChannelFactory : IGrpcChannelFactory
{
   private readonly IServiceProvider _serviceProvider;
   private readonly ILogger<GrpcChannelFactory> _logger;
   private readonly IOptionsMonitor<GrpcClientConfiguration> _configuration;
   private readonly Dictionary<string, ChannelInstance> _channels = [];
   private readonly Dictionary<string, long> _generations = [];
   private static object _lock = new();

   public GrpcChannelFactory(
      IServiceProvider serviceProvider,
      ILogger<GrpcChannelFactory> logger,
      IOptionsMonitor<GrpcClientConfiguration> configuration)
   {
      _serviceProvider = serviceProvider;
      _logger = logger;
      _configuration = configuration;
   }

   private ChannelInstance CreateChannelInstance(string url, Activity? activity, long generation)
   {
      activity?.AddEvent(new ActivityEvent("Channel creation started"));
      activity?.AddTag(nameof(GrpcClientConfiguration), JsonSerializer.Serialize(_configuration.CurrentValue));

      var defaultMethodConfig = new MethodConfig
      {
         Names = { MethodName.Default },
         RetryPolicy = new RetryPolicy
         {
            MaxAttempts = _configuration.CurrentValue.RetryMaxAttempts,
            InitialBackoff = TimeSpan.FromSeconds(5),
            MaxBackoff = TimeSpan.FromSeconds(30),
            BackoffMultiplier = 1.5,
            RetryableStatusCodes = { StatusCode.Unavailable }
         }
      };

      var dnsUrl = _configuration.CurrentValue.UseSecureSslChannel ? url.Replace("https://", "dns:///") : url.Replace("http://", "dns:///");
      activity?.AddTag(nameof(dnsUrl), dnsUrl);

      var channel = GrpcChannel.ForAddress(dnsUrl, new GrpcChannelOptions
      {
         Credentials = _configuration.CurrentValue.UseSecureSslChannel ? ChannelCredentials.SecureSsl : ChannelCredentials.Insecure,
         HttpHandler = new SocketsHttpHandler
         {
            PooledConnectionLifetime = TimeSpan.FromMinutes(_configuration.CurrentValue.PooledConnectionLifetimeMinutes),
            PooledConnectionIdleTimeout = TimeSpan.FromSeconds(_configuration.CurrentValue.PooledConnectionIdleTimeoutSeconds),
            MaxConnectionsPerServer = _configuration.CurrentValue.MaxConnectionsPerServer,
            EnableMultipleHttp2Connections = true
         },
         ServiceConfig = new ServiceConfig
         {
            MethodConfigs = { defaultMethodConfig },
            LoadBalancingConfigs = { new RoundRobinConfig() }
         },
         ServiceProvider = _serviceProvider
      });

      CallInvoker? callInvoker = null;
      return new ChannelInstance(channel, url, callInvoker, generation);
   }

   private async Task<ChannelInstance> GetChannelInstanceAsync(string channelUrl, Activity? activity)
   {
      ChannelInstance instance;
      long generation;

      lock (_lock)
      {
         if (!_channels.ContainsKey(channelUrl))
         {
            activity?.AddEvent(new ActivityEvent("New ChannelInstance will be created"));
            generation = _generations.TryGetValue(channelUrl, out var gen) ? gen + 1 : 1;
            _generations[channelUrl] = generation;
            instance = CreateChannelInstance(channelUrl, activity, generation);
            activity?.AddTag("Generation", generation);
            activity?.AddEvent(new ActivityEvent("New ChannelInstance created"));
            _channels.Add(channelUrl, instance);
            return instance;
         }
         else
         {
            activity?.AddEvent(new ActivityEvent("Existing ChannelInstance found"));
            instance = _channels[channelUrl];
            generation = instance.Generation;
            activity?.AddTag("Generationin", generation);
            activity?.AddTag("ChannelState", instance.Channel.State);
            if (instance.Channel.State is ConnectivityState.Shutdown or ConnectivityState.TransientFailure)
            {
               activity?.AddEvent(new ActivityEvent("Bad state"));
               return Recreate(instance, activity);
            }
         }
      }

      try
      {
         activity?.AddEvent(new ActivityEvent("Connection check"));
         using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
         await instance.Channel.ConnectAsync(cts.Token);
         activity?.AddEvent(new ActivityEvent("Connection OK"));
         return instance;
      }
      catch (Exception ex)
      {
         activity?.AddEvent(new ActivityEvent("ConnectAsync failed, checking generation"));
         activity?.AddTag("ConnectExceptionType", ex.GetType().FullName);
         activity?.AddTag("ConnectException", ex.Message);
      }

      lock (_lock)
      {
         var currentInstance = _channels[channelUrl];
         if (currentInstance.Generation == generation)
         {
            activity?.AddEvent(new ActivityEvent("Generation not changed, recreating"));
            return Recreate(currentInstance, activity);
         }
         else
         {
            activity?.AddEvent(new ActivityEvent("Channel already replaced by another thread"));
            return _channels[channelUrl];
         }
      }
   }

   private ChannelInstance Recreate(ChannelInstance instance, Activity? activity)
   {
      activity?.AddEvent(new ActivityEvent("Recreating"));
      var channelUrl = instance.ChannelUrl;
      instance.Dispose();
      activity?.AddEvent(new ActivityEvent("Disposed"));
      var newGeneration = ++_generations[channelUrl];
      var newInstance = CreateChannelInstance(channelUrl, activity, newGeneration);
      activity?.AddEvent(new ActivityEvent("Recreated"));
      _channels[channelUrl] = newInstance;
      return newInstance;
   }

   public CallInvoker GetInvoker(string channelUrl)
   {
      using (var activity = Instrumentation.ActivitySource.StartActivity(nameof(GetInvoker)))
      {
         activity?.AddTag(nameof(channelUrl), channelUrl);
         try
         {
            if (string.IsNullOrWhiteSpace(channelUrl))
            {
               activity?.AddEvent(new ActivityEvent("ChannelUrl is null or empty"));
               throw new Exception("GrpcChannelCreationFailed");
            }

            var instance = Task.Run(() => GetChannelInstanceAsync(channelUrl, activity)).Result;

            if (instance.CallInvoker != null)
            {
               return instance.CallInvoker;
            }
            
            return instance.Channel.CreateCallInvoker();
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "GetInvoker failed. TraceId: {TraceId}", activity?.TraceId);
            throw;
         }
      }
   }
}
