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

[InjectableGeneric(lifetime: ServiceLifetime.Singleton)]
public class GrpcClientFactory<TGrpcClient>(
    IGrpcChannelFactory channelFactory, 
    ILogger<GrpcClientFactory<TGrpcClient>> logger) : IGrpcClientFactory<TGrpcClient>
    where TGrpcClient : ClientBase<TGrpcClient>
{
    public TGrpcClient GetClient(string channelUrl)
    {
        using (var activity = Instrumentation.ActivitySource.StartActivity(nameof(GetClient)))
        {
            activity?.AddTag(nameof(channelUrl), channelUrl);
            activity?.AddTag("clientType", typeof(TGrpcClient).FullName);
            try
            {
                if (string.IsNullOrWhiteSpace(channelUrl))
                {
                    activity?.AddEvent(new ActivityEvent("ChannelUrl is null or empty"));
                    throw new Exception("GrpcClientCreationFailed: ChannelUrl can not be null or empty.");
                }

                var invoker = channelFactory.GetInvoker(channelUrl);

                activity?.AddEvent(new ActivityEvent("Create instance with CallInvoker"));
                var client = (TGrpcClient?)Activator.CreateInstance(typeof(TGrpcClient), invoker);

                if (client is null)
                {
                    activity?.AddEvent(new ActivityEvent("Client not created"));
                    throw new Exception("GrpcClientCreationFailed: Activator returned null.");
                }

                activity?.AddEvent(new ActivityEvent("Client created"));
                return client;
            }
            catch (Exception ex)
            {
                activity?.AddTag("error", true);
                activity?.AddTag("exception.message", ex.Message);
                logger.LogError(ex, "GetClient failed. TraceId: {TraceId}", activity?.TraceId);
                throw;
            }
        }
    }
}
