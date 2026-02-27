using JK.Platform.Core.AspNetCore.Abstractions.Kestrel;
using Microsoft.Extensions.Configuration;

namespace JK.Platform.Core.AspNetCore.Abstractions;

public interface IWebHostConfigurator
{
   void ConfigureKestrelOptions(KestrelOptions options, IConfigurationRoot configuration);
}