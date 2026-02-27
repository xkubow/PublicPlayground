using System.Net;
using System.Text;
using JK.Platform.Core.AspNetCore.Abstractions.Kestrel;
using JK.Platform.Core.Configuration;

namespace JK.Platform.Core.AspNetCore.Abstractions;

public abstract class WebHostConfiguration: IAppConfiguration
{
    protected int? _port;

    public abstract string SectionName { get; }

    public virtual KestrelListenIPType ListenType { get; set; } = KestrelListenIPType.Any;

    public virtual KestrelListenHttpProtocols Protocols { get; set; } = KestrelListenHttpProtocols.Http1AndHttp2AndHttp3;

    public virtual IPAddress? IpAddress { get; set; }

    public virtual int Port { get; set; }

    public virtual string? Hostname { get; set; }

    public virtual bool IncludePortInRequiredHost { get; set; } = true;

    public virtual bool UseHttps { get; set; } = false;

    public virtual string GetHost()
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(Hostname))
            sb.Append(Hostname);
        else
            switch (ListenType)
            {
                case KestrelListenIPType.Any:
                    sb.Append("*");
                    break;

                case KestrelListenIPType.Specific:
                    if (IpAddress == null)
                        throw new ArgumentException($"IP Address must not be null for type Specific");

                    sb.Append(IpAddress);
                    break;

                case KestrelListenIPType.Localhost:
                    sb.Append("localhost");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

        if (IncludePortInRequiredHost)
        {
            sb.Append(":");
            sb.Append(GetPort());
        }

        return sb.ToString();
    }

    public virtual int GetPort()
    {
        if (_port.HasValue)
            return _port.Value;

        var urlsString = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
        if (!string.IsNullOrEmpty(urlsString))
        {
            var urls = urlsString.Split(";");
            if (urls.Length > 1)
                throw new ArgumentException("Multiple URLS configuration is not supported");

            var uri = new Uri(urls.First());
            _port = uri.Port;
        }
        else
        {
            _port = Port;
        }

        return _port.Value;
    }
}