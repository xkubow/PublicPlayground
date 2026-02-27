using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.DependencyInjection.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InjectableGenericAttribute : Attribute, IInjectableAttribute
{
   public object? Key { get; set; }
   public int Priority { get; set; }
   public ServiceLifetime Lifetime { get; set; }

   public InjectableGenericAttribute(object? key = null, int priority = 0, ServiceLifetime lifetime = ServiceLifetime.Transient)
   {
      Key = key;
      Priority = priority;
      Lifetime = lifetime;
   }

   public IEnumerable<ServiceRegistration> ToServiceRegistration(Type targetType)
   {
      var interfaceTypes = targetType.GetInterfaces().Where(x => x.IsGenericType && !x.GetCustomAttributes(typeof(CommonInterfaceAttribute), false).Any()).ToArray();

      foreach (var interfaceType in interfaceTypes)
      {
         yield return new ServiceRegistration()
         {
            Key = Key,
            ImplementationType = targetType.GetGenericTypeDefinition(),
            InterfaceType = interfaceType.GetGenericTypeDefinition(),
            Priority = Priority,
            Lifetime = Lifetime,
            Multiple = false
         };
      }
   }
}