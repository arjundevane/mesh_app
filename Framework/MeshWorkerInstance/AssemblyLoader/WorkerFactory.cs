using Google.Protobuf;
using MeshApp.WorkStructure;
using MeshApp.WorkStructure.Interfaces;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace MeshApp.WorkerInstance.AssemblyLoader
{
    public static class AssemblyCache
    {
        public static Dictionary<string, Assembly> GlobalCache = new Dictionary<string, Assembly>();
    }

    public class WorkerFactory
    {
        private readonly AssemblyLoadContext _context;
        private readonly ProcessStepInfo _processInfo;
        private Assembly? _loadedAssembly;

        public WorkerFactory(AssemblyLoadContext context, ProcessStepInfo info)
        {
            _context = context;
            _processInfo = info;
            LoadAssemblies();
        }

        private void LoadAssemblies()
        {
            // Get starting assembly
            var reader = new AssemblyStorageReader(_context, _processInfo.FilePath);
            var assembly = reader.LoadAssembly();
            var assemblyType = assembly
                                .GetExportedTypes()
                                .Where(t => t.Name.Equals(_processInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                                .FirstOrDefault();

            if (assemblyType == null)
                throw new Exception($"Worker assembly with name {_processInfo.Name} not found!");

            //var requiredAssembiles = assembly.GetReferencedAssemblies();
            //foreach (var requiredAssembly in requiredAssembiles)
            //{
            //    Console.WriteLine($"Required Assembly ==> {requiredAssembly.FullName}");
            //}

            _loadedAssembly = assembly;
        }

        public Tuple<Type, Type> GetIOTypes()
        {
            var inputType = Type.GetType(_processInfo.RequestType, assemblyResolver: AssemblyContextAssemblyResolver, typeResolver: AssemblyContextTypeResolver) ?? typeof(object);
            var outputType = Type.GetType(_processInfo.ResponseType, assemblyResolver: AssemblyContextAssemblyResolver, typeResolver: AssemblyContextTypeResolver) ?? typeof(object);

            return new Tuple<Type, Type>(inputType ?? typeof(object), outputType ?? typeof(object));
        }

        /// <summary>
        /// To be used when getting types from the current assembly context
        /// </summary>
        /// <returns></returns>
        private Assembly? AssemblyContextAssemblyResolver(AssemblyName assembly)
        {
            return _context.Assemblies.Where(a => a.FullName == assembly.FullName).FirstOrDefault();
        }

        /// <summary>
        /// To be used when getting types from the current assembly context
        /// </summary>
        /// <returns></returns>
        private Type? AssemblyContextTypeResolver(Assembly? assembly, string typeName, bool whatIsThis)
        {
            if (assembly == null)
                return null;

            var exportedTypes = assembly.GetExportedTypes().Where(t => t.FullName == typeName).FirstOrDefault();

            return exportedTypes;
        }

        public IWorker<TRequest, TResponse> GetWorker<TRequest, TResponse>(AssemblyLoadContext context, ProcessStepInfo info)
            where TRequest : IMessage<TRequest>
            where TResponse : IMessage<TResponse>
        {

            if (_loadedAssembly == null)
                throw new Exception($"No assembly loader in {nameof(WorkerFactory)}. This method should not be invoked when {nameof(WorkerFactory)} threw an error.");

            var assemblyType = _loadedAssembly?
                                .GetExportedTypes()
                                .Where(t => t.Name.Equals(_processInfo.Name, StringComparison.InvariantCultureIgnoreCase))
                                .FirstOrDefault();

            if (assemblyType == null)
                throw new Exception($"Worker assembly with name {_processInfo.Name} not found!");

            // Build object and invoke
            var ctor = assemblyType.GetConstructors();
            // !!! This breaks if there are more than one constructor in the same IWorker implementation !!!
            // TODO - Update the IWorker to require a parameter-less constructor at compile-time
            var obj = (IWorker<TRequest, TResponse>)ctor[0].Invoke(null);
            return obj;
        }
    }

    public class AssemblyStorageReader
    {
        private readonly AssemblyLoadContext _context;
        private readonly string _basePath;

        public AssemblyStorageReader(AssemblyLoadContext context, string filePath)
        {
            _context = context;
            _basePath = filePath;
            // Order of lookup
            // 1. In the same folder where original assembly is
            _context.Resolving += this.LoadFromSameFolder;
            // 2. Nuget packages
            _context.Resolving += NugetReader.NugetResolverAssemblyContext;
        }

        public Assembly LoadAssembly()
        {
            var assembly = _context.LoadFromAssemblyPath(_basePath);
            return assembly;
        }

        public Assembly? LoadFromSameFolder(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            Console.WriteLine($"In {nameof(AssemblyStorageReader)}.{nameof(LoadFromSameFolder)} ==> {assemblyName.Name}");

            if (assemblyName == null || assemblyName.Name == null || assemblyName.FullName == null)
                return null;

            // Check if already loaded, this should not happen
            if (context.Assemblies.Select(a => a.FullName).Any(a => a?.Contains(assemblyName.FullName) ?? false))
            {
                throw new Exception("Circular reference in NugetResolverAssemblyContext");
            }

            // Check in global cache
            if (AssemblyCache.GlobalCache.ContainsKey(assemblyName.FullName))
            {
                return AssemblyCache.GlobalCache[assemblyName.FullName];
            }

            // Not found yet, walk through the filesystem
            var files = Directory.GetFiles(Path.GetDirectoryName(_basePath) ?? throw new Exception($"Error while parsing path: {_basePath}"));

            var assemblyPath = files.Where(f => f.Contains(assemblyName.Name)).FirstOrDefault();

            if (assemblyPath == null)
            {
                return null;
                // throw new NotImplementedException("Assembly not found in the local filesystem. Pull from Nuget not implemented yet.");
            }

            var assembly = context.LoadFromAssemblyPath(assemblyPath);
            if (assembly != null && assembly.FullName != null)
                AssemblyCache.GlobalCache.Add(assembly.FullName, assembly);
            return assembly;
        }
    }

    public class NugetReader
    {
        private static string _nugetRepoBase = "C:\\Users\\arjun\\.nuget\\packages\\";
        private static string _frameworkPath = "lib\\net6.0";

        public NugetReader() { }

        public static Assembly? NugetResolverAssemblyContext(AssemblyLoadContext context, AssemblyName assemblyName)
        {
            Console.WriteLine($"In {nameof(NugetResolverAssemblyContext)} ==> {assemblyName.Name}");

            // Check if already loaded, this should not happen
            if (context.Assemblies.Select(a => a.FullName).Any(a => a?.Contains(assemblyName.FullName) ?? false))
            {
                throw new Exception("Circular reference in NugetResolverAssemblyContext");
            }

            // Check in global cache
            if (AssemblyCache.GlobalCache.ContainsKey(assemblyName.FullName))
            {
                return AssemblyCache.GlobalCache[assemblyName.FullName];
            }

            // Not found yet, walk through the filesystem
            var assemblyPath = GetPathByAssemblyName(assemblyName.FullName, _nugetRepoBase);

            if (assemblyPath == null)
            {
                return null;
                // throw new NotImplementedException("Assembly not found in the local filesystem. Pull from Nuget not implemented yet.");
            }

            var assembly = context.LoadFromAssemblyPath(assemblyPath);
            if (assembly != null && assembly.FullName != null)
                AssemblyCache.GlobalCache.Add(assembly.FullName, assembly);
            return assembly;
        }

        private static string? GetPathByAssemblyName(string requestedAssemblyName, string basePath)
        {
            var assemblyName = new AssemblyName(requestedAssemblyName);
            var files = Directory.EnumerateDirectories(basePath);

            if (assemblyName == null || assemblyName.Name == null || assemblyName.Version == null)
                return null;

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
