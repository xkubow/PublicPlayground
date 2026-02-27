namespace JK.Platform.Core.AspNetCore.Abstractions.Kestrel;

//should match Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols

[Flags]
public enum KestrelListenHttpProtocols
{
   None = 0,
   Http1 = 1,
   Http2 = 2,
   Http1AndHttp2 = 3,
   Http3 = 4,
   Http1AndHttp2AndHttp3 = 7,
}