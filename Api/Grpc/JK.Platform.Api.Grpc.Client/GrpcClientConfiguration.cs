using JK.Platform.Core.Configuration;

namespace JK.Platform.Api.Grpc.Client;

public class GrpcClientConfiguration : IAppConfiguration
{
   public string SectionName => "GrpcClient";
   public bool LogRequestData { get; set; }
   public bool LogResponseData { get; set; }
   public int MaxConnectionsPerServer { get; set; } = 10;
   public int PooledConnectionLifetimeMinutes { get; set; } = 10;
   public int PooledConnectionIdleTimeoutSeconds { get; set; } = 30;
   public int RetryMaxAttempts { get; set; } = 3;
   public bool UseSecureSslChannel { get; set; }
}