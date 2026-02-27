using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection;

namespace JK.Platform.Core.Instrumentation;

public class AssemblyInstrumentation : IDisposable
{
   public string Name { get; }
   public string Version { get; }
   public ActivitySource ActivitySource { get; }
   public Meter Meter { get; }

   public AssemblyInstrumentation()
   {
      var name = Assembly.GetCallingAssembly().GetName();

      Name = name.Name ?? name.FullName;
      Version = name.Version?.ToString() ?? "Unknown";

      ActivitySource = new(Name, Version);
      Meter = new Meter(Name, Version);
   }

   public void Dispose()
   {
      ActivitySource.Dispose();
      Meter.Dispose();
   }
}