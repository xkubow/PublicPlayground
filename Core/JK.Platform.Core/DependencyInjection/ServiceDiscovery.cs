using System.Reflection;
using System.Text;
using JK.Platform.Core.DependencyInjection.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.DependencyInjection;

public static class ServiceDiscovery
{
   public static IServiceCollection RegisterInjectableServices(this IServiceCollection serviceCollection)
   {
      var assemblies = AppDomain.CurrentDomain.GetAppDomainAssemblies();

      var serviceCollectionRegistrations = new List<ServiceRegistration>();
      var stringBuilder = new StringBuilder();

      try
      {
         Register<InjectableAttribute>(assemblies, serviceCollectionRegistrations, stringBuilder, "Injectable types");
         Register<InjectableGenericAttribute>(assemblies, serviceCollectionRegistrations, stringBuilder, "InjectableGeneric types");

         stringBuilder.AppendLine("----- ServiceCollection registrations");
         serviceCollection.RegisterServicesInternal(serviceCollectionRegistrations.ToArray(), stringBuilder);
         stringBuilder.AppendLine("-----");
      }
      finally
      {
         if (stringBuilder.Length > 0)
            Console.WriteLine(stringBuilder.ToString());
      }

      return serviceCollection;
   }

   private static void Register<TAttribute>(System.Collections.Concurrent.ConcurrentDictionary<string, Assembly> assemblies, List<ServiceRegistration> registrations, StringBuilder stringBuilder, string label)
       where TAttribute : Attribute, IInjectableAttribute
   {
       var injectableTypes = assemblies
           .SelectMany(x => x.Value.GetTypes()
               .Select(t => (Type: t, Attribute: t.GetCustomAttribute<TAttribute>()!))
               .Where(x => x.Attribute != null));

       stringBuilder.AppendLine($"----- {label}");
       foreach (var (type, attribute) in injectableTypes)
       {
           var registrationsArray = attribute.ToServiceRegistration(type).ToArray();
           
           stringBuilder.AppendLine($"Registration: {type.FullName}, life time: {attribute.Lifetime}, key: {attribute.Key}, priority: {attribute.Priority}. Prepared registrations: {registrationsArray.Length}.");
           registrations.AddRange(registrationsArray);
       }
       stringBuilder.AppendLine("-----");
   }

   internal static void RegisterServicesInternal(this IServiceCollection serviceCollection, IEnumerable<ServiceRegistration> registrations, StringBuilder stringBuilder)
   {
      var serviceRegistrations = registrations as ServiceRegistration[] ?? registrations.ToArray();

      var groupedRegistrations = serviceRegistrations
         .GroupBy(r => new { r.InterfaceType, r.Key });

      foreach (var group in groupedRegistrations)
      {
         var registrationsToProcess = group.AsEnumerable();

         var firstRegistration = group.First();
         if (!firstRegistration.Multiple)
         {
            var maxPriority = group.Max(r => r.Priority);
            
            // In the original code, a registration is skipped with "Higher priority registration" 
            // ONLY if there is another registration with STRICTLY HIGHER priority.
            // If multiple have the same max priority, they all pass this check.
            
            var highestPriorityRegistrations = group.Where(r => r.Priority == maxPriority).ToList();
            var lowerPriorityRegistrations = group.Where(r => r.Priority < maxPriority);

            foreach (var skipped in lowerPriorityRegistrations)
            {
               stringBuilder.AppendLine($"Higher priority registration - skipping registration of {skipped.ImplementationType.FullName}");
            }

            registrationsToProcess = highestPriorityRegistrations;
         }

         foreach (var registration in registrationsToProcess)
         {
            var serviceDescriptor = serviceCollection.FirstOrDefault(x => x.ServiceType == registration.InterfaceType);
            if (serviceDescriptor == null)
            {
               serviceCollection.Add(new ServiceDescriptor(registration.InterfaceType, registration.Key, registration.ImplementationType, registration.Lifetime));
               stringBuilder.AppendLine($"Registration: lifetime: {registration.Lifetime}, interface: {registration.InterfaceType.FullName}, implementation: {registration.ImplementationType.FullName}");
               continue;
            }

            if (registration.Multiple || registration.Key != null)
            {
               serviceCollection.Add(new ServiceDescriptor(registration.InterfaceType, registration.Key, registration.ImplementationType, registration.Lifetime));
               stringBuilder.AppendLine($"MultipleRegistration: lifetime: {registration.Lifetime}, interface: {registration.InterfaceType.FullName}, implementation: {registration.ImplementationType.FullName}");
               continue;
            }

            stringBuilder.AppendLine($"Unable to register service: {registration.ImplementationType.FullName}, possible duplicity in service registrations of interface: {registration.InterfaceType.FullName}");
         }
      }
   }
}