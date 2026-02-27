using Grpc.Core;

namespace JK.Platform.Api.Grpc.Client.Factory;

public interface IGrpcChannelFactory
{
    CallInvoker GetInvoker(string channelUrl);
}
