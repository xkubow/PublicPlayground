using Grpc.Core;
using Grpc.Net.Client;

namespace JK.Platform.Api.Grpc.Client.Factory;

public class ChannelInstance : IDisposable
{
   internal GrpcChannel Channel { get; }
   internal CallInvoker? CallInvoker { get; }
   internal string ChannelUrl { get; }
   internal long Generation { get; }

   internal ChannelInstance(GrpcChannel channel, string channelUrl, CallInvoker? invoker, long generation)

   {
      Channel = channel;
      ChannelUrl = channelUrl;
      CallInvoker = invoker;
      Generation = generation;
   }

   public void Dispose()
   {
      Channel.Dispose();
   }
}