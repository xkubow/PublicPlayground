using System.Diagnostics;
using System.Diagnostics.Metrics;
using JK.Platform.Core.Instrumentation;

namespace JK.Platform.Api.Grpc.Client;

internal static class Instrumentation
{
   private static readonly AssemblyInstrumentation _instrumentation;
   public static ActivitySource ActivitySource => _instrumentation.ActivitySource;
   public static Meter Meter => _instrumentation.Meter;

   static Instrumentation()
   {
      _instrumentation = new AssemblyInstrumentation();
   }
}