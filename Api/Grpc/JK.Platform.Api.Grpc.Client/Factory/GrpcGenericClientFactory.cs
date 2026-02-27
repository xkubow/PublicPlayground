using Grpc.Core;
using JK.Platform.Api.Grpc.Client.Factory;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Api.Grpc.Client.Factory;

[Injectable(lifetime: ServiceLifetime.Singleton)]
public class GrpcGenericClientFactory(IGrpcChannelFactory channelFactory) : IGrpcGenericClientFactory
{
    public IGrpcGenericClient GetClient(string channelUrl)
    {
        return new GrpcGenericClient(channelFactory.GetInvoker(channelUrl));
    }

    private class GrpcGenericClient(CallInvoker invoker) : IGrpcGenericClient
    {
        public async Task<TResponse> CallAsync<TRequest, TResponse>(
            Method<TRequest, TResponse> method,
            TRequest request,
            CallOptions options = default)
            where TRequest : class
            where TResponse : class
        {
            var call = invoker.AsyncUnaryCall(method, null, options, request);
            return await call.ResponseAsync;
        }

        public async Task<byte[]> CallRawAsync(string serviceName, string methodName, byte[] request, CallOptions options = default)
        {
            var method = new Method<byte[], byte[]>(
                MethodType.Unary,
                serviceName,
                methodName,
                Marshallers.Create(r => r, r => r),
                Marshallers.Create(r => r, r => r));

            var call = invoker.AsyncUnaryCall(method, null, options, request);
            return await call.ResponseAsync;
        }
    }
}
