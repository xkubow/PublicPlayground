using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace JK.Platform.Core.AspNetCore.Abstractions.Kestrel;

public class KestrelOptions
{
    private record ListenEntry(KestrelListenIPType Type, IPAddress? IpAddress, int Port, KestrelListenHttpProtocols Protocols, bool UseHttps);

    private readonly List<ListenEntry> _listenEntries = [];

    public void Listen(IPAddress address, int port, KestrelListenHttpProtocols httpProtocol = KestrelListenHttpProtocols.Http1AndHttp2AndHttp3, bool useHttps = false)
    {
        _listenEntries.Add(new ListenEntry(KestrelListenIPType.Specific, address, port, httpProtocol, useHttps));
    }

    public void ListenAnyIp(int port, KestrelListenHttpProtocols httpProtocol = KestrelListenHttpProtocols.Http1AndHttp2AndHttp3, bool useHttps = false)
    {
        _listenEntries.Add(new ListenEntry(KestrelListenIPType.Any, null, port, httpProtocol, useHttps));
    }

    public void ListenLocalhost(int port, KestrelListenHttpProtocols httpProtocol = KestrelListenHttpProtocols.Http1AndHttp2AndHttp3, bool useHttps = false)
    {
        _listenEntries.Add(new ListenEntry(KestrelListenIPType.Localhost, null, port, httpProtocol, useHttps));
    }

    public void Apply(KestrelServerOptions options)
    {
        foreach (var anyListenEntry in _listenEntries.Where(x => x.Type == KestrelListenIPType.Any))
        {
            // IPv6 support disabled
            // if (System.Net.Sockets.Socket.OSSupportsIPv6)
            //    options.Listen(IPAddress.IPv6Any, anyListenEntry.Port, listenOptions => listenOptions.Protocols = (HttpProtocols)(int)anyListenEntry.Protocols);
            options.Listen(IPAddress.Any, anyListenEntry.Port, listenOptions =>
            {
                listenOptions.Protocols = (HttpProtocols)(int)anyListenEntry.Protocols;
                if (anyListenEntry.UseHttps)
                    listenOptions.UseHttps();
            });
        }

        foreach (var specificListenEntry in _listenEntries.Where(x => x is { Type: KestrelListenIPType.Specific, IpAddress: not null }))
        {
            options.Listen(specificListenEntry.IpAddress!, specificListenEntry.Port, listenOptions =>
            {
                listenOptions.Protocols = (HttpProtocols)(int)specificListenEntry.Protocols;
                if (specificListenEntry.UseHttps)
                    listenOptions.UseHttps();
            });
        }

        foreach (var localhostListenEntry in _listenEntries.Where(x => x.Type == KestrelListenIPType.Localhost))
        {
            options.ListenLocalhost(localhostListenEntry.Port, listenOptions =>
            {
                listenOptions.Protocols = (HttpProtocols)(int)localhostListenEntry.Protocols;
                if (localhostListenEntry.UseHttps)
                    listenOptions.UseHttps();
            });
        }
    }
}