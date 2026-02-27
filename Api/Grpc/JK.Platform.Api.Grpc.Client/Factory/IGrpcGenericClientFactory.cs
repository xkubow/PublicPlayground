namespace JK.Platform.Api.Grpc.Client.Factory;

public interface IGrpcGenericClientFactory
{
    IGrpcGenericClient GetClient(string channelUrl);
}
