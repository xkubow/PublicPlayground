using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using JK.Platform.Core.Constants;

namespace JK.Platform.Core;

public static class CoreExtensions
{
    public static bool IsProjectAssembly(this Assembly assembly) => assembly.FullName?.StartsWith($"{CoreConstants.CompanyName}.") == true;

    public static bool IsProjectAssembly(this AssemblyName assemblyName) => assemblyName.FullName?.StartsWith($"{CoreConstants.CompanyName}.") == true;

    public static ConcurrentDictionary<string, Assembly> GetAppDomainAssemblies(this AppDomain appDomain)
    {
        var appDomainAssemblies = new ConcurrentDictionary<string, Assembly>();
        var domainAssemblies = appDomain.GetAssemblies();
        // 1. Already loaded in the AppDomain
        foreach (var assembly in domainAssemblies.Where(a => a.IsProjectAssembly()))
            appDomainAssemblies.TryAdd(assembly.FullName!, assembly);

        // 2. Load DLLs from the base directory safely
        var assemblyFolder = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));
        // var assemblyFolder = AppDomain.CurrentDomain.BaseDirectory;
        var files = Directory.GetFiles(assemblyFolder, "*.dll");
        foreach (var file in files)
        {
            try
            {
                // Use LoadName to stay in the default context if possible
                var assemblyName = AssemblyName.GetAssemblyName(file);
                if (assemblyName.IsProjectAssembly())
                {
                    var assembly = Assembly.Load(assemblyName);
                    appDomainAssemblies.TryAdd(assembly.FullName!, assembly);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"\n>> Skip file {file}: {ex.Message}");
            }
        }

        // Note: In modern .NET, recursive manual loading is often unnecessary
        // if all DLLs are in the output folder, but if kept, ensure it's safe.
        // foreach (var assembly in domainAssemblies)
        //     LoadReferencedAssemblies(assembly, appDomainAssemblies);
        return appDomainAssemblies;
    }

    private static void LoadReferencedAssemblies(this Assembly assembly, ConcurrentDictionary<string, Assembly> loadedAssemblies)
    {
        foreach (var referenceAssembly in assembly.GetReferencedAssemblies().Where(a => a.IsProjectAssembly()))
        {
            try
            {
                var referencedAssembly = Assembly.Load(referenceAssembly);
                referencedAssembly.LoadReferencedAssemblies(loadedAssemblies);
                loadedAssemblies.TryAdd(referenceAssembly.FullName, referencedAssembly);
                Debug.WriteLine($"\n>> Referenced assembly => {referenceAssembly.FullName}");
            }
            catch (Exception)
            {
                Debug.WriteLine($"\n>> Unable to load referenced assembly {referenceAssembly.FullName}");
            }
        }
    }

    public static List<Type> GetImplementationsWithDefaultConstructor<TType>(this ConcurrentDictionary<string, Assembly> assemblies)
    {
        return assemblies.Values
            .SelectMany(a => a.GetTypes().Where(t => typeof(TType).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null && !t.IsAbstract && t.IsClass && t.IsPublic))
            .ToList();
    }

    public static List<Type> GetImplementations<TType>(this ConcurrentDictionary<string, Assembly> assemblies)
    {
        return assemblies.Values
            .SelectMany(a => a.GetTypes().Where(t => typeof(TType).IsAssignableFrom(t) && !t.IsAbstract && t.IsClass))
            .ToList();
    }
}