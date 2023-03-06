using MeshApp.WorkInterface;
using MeshApp.WorkStructure;
using System.Reflection;
using System.Runtime.Loader;

namespace WorkerInstance.AssemblyLoader
{
    public static class AssemblyCache
    {
        public static Dictionary<string, Assembly> GlobalCache = new Dictionary<string, Assembly>();
    }

    public class WorkerFactory
    {
        public WorkerFactory() { }

        public static IWorker<TRequest, TResponse> GetWorker<TRequest, TResponse>(AssemblyLoadContext context, ProcessStepInfo info)
        {
            // Get starting assembly
            var reader = new AssemblyStorageReader(context);
            var assembly = reader.LoadAssembly(info.FilePath);
            var assemblyType = assembly
                                .GetExportedTypes()
                                .Where(t => t.Name.Equals(info.Name, StringComparison.InvariantCultureIgnoreCase))
                                .FirstOrDefault();

            if (assemblyType == null)
                throw new Exception($"Worker assembly with name {info.Name} not found!");

            //Console.WriteLine($"Found starter assembly ==> {assembly.FullName}");

            // Find and load referenced modules
            var requiredAssembiles = assembly.GetReferencedAssemblies();
            //foreach (var requiredAssembly in requiredAssembiles)
            //{
            //    Console.WriteLine($"Required Assembly ==> {requiredAssembly.FullName}");
            //}

            // Build object and invoke
            var ctor = assemblyType.GetConstructors();
            var obj = (IWorker<TRequest, TResponse>)ctor[0].Invoke(null);
            return obj;
        }
    }

    public class AssemblyStorageReader
    {
        private readonly AssemblyLoadContext _context;

        public AssemblyStorageReader(AssemblyLoadContext context)
        {
            _context = context;
            _context.Resolving += NugetReader.NugetResolverAssemblyContext;
        }

        public Assembly LoadAssembly(string assemblyPath)
        {
            var assembly = _context.LoadFromAssemblyPath(assemblyPath);
            return assembly;
        }
    }

    public class NugetReader
    {
        private static string _nugetRepoBase = "C:\\Users\\arjun\\.nuget\\packages\\";
        private static string _frameworkPath = "lib\\net6.0";

        public NugetReader() { }

        public static Assembly NugetAppDomainResolver(object sender, ResolveEventArgs args)
        {
            var domain = (AppDomain)sender;

            Console.WriteLine($"In {nameof(NugetAppDomainResolver)} ==> {args.Name} requested by {args.RequestingAssembly}");

            // Check if already loaded
            if (domain.GetAssemblies().Select(a => a.FullName).Any(a => a.Contains(args.Name)))
            {
                throw new Exception("Circular reference in NugetResolver");
            }

            // Check in global cache
            if (AssemblyCache.GlobalCache.ContainsKey(args.Name))
            {
                return AssemblyCache.GlobalCache[args.Name];
            }

            // Not found yet, walk through the filesystem
            var assemblyPath = GetPathByAssemblyName(args.Name, _nugetRepoBase);

            if (assemblyPath == null)
            {
                throw new NotImplementedException("Assembly not found in the local filesystem. Pull from Nuget not implemented yet.");
            }

            var assembly = Assembly.LoadFrom(assemblyPath);
            AssemblyCache.GlobalCache.Add(assembly.FullName, assembly);
            return assembly;
        }

        public static Assembly NugetResolverAssemblyContext(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            Console.WriteLine($"In {nameof(NugetResolverAssemblyContext)} ==> {assemblyName.Name}");

            // Check if already loaded, this should not happen
            if (context.Assemblies.Select(a => a.FullName).Any(a => a.Contains(assemblyName.Name)))
            {
                throw new Exception("Circular reference in NugetResolverAssemblyContext");
            }

            // Check in global cache
            if (AssemblyCache.GlobalCache.ContainsKey(assemblyName.Name))
            {
                return AssemblyCache.GlobalCache[assemblyName.Name];
            }

            // Not found yet, walk through the filesystem
            var assemblyPath = GetPathByAssemblyName(assemblyName.FullName, _nugetRepoBase);

            if (assemblyPath == null)
            {
                throw new NotImplementedException("Assembly not found in the local filesystem. Pull from Nuget not implemented yet.");
            }

            var assembly = context.LoadFromAssemblyPath(assemblyPath);
            AssemblyCache.GlobalCache.Add(assembly.FullName, assembly);
            return assembly;
        }

        private static string GetPathByAssemblyName(string requestedAssemblyName, string basePath)
        {
            var assemblyName = new AssemblyName(requestedAssemblyName);
            var files = Directory.EnumerateDirectories(basePath);

            // Check if base package exists
            if (files.Any(f => f.Contains(assemblyName.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                // Check versions
                var versions = Directory.EnumerateDirectories($"{basePath}\\{assemblyName.Name.ToLowerInvariant()}");
                var requiredVersion = $"{assemblyName.Version.Major}.{assemblyName.Version.Minor}.{assemblyName.Version.Build}";
                if (versions.Any(v => v.Contains(requiredVersion, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return $"{basePath}\\{assemblyName.Name.ToLowerInvariant()}\\{requiredVersion}\\{_frameworkPath}\\{assemblyName.Name}.dll";
                }
            }

            return null;
        }
    }
}
