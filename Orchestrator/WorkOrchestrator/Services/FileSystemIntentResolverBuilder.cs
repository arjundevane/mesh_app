﻿using MeshApp.WorkInterface;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
using System.Reflection;
using System.Timers;

namespace WorkOrchestrator.Services
{
    public class FileSystemIntentResolverBuilder : IIntentResolverBuilder
    {
        private readonly IConfiguration _config;
        private readonly ILogger<FileSystemIntentResolverBuilder> _logger;

        public FileSystemIntentResolverBuilder(IConfiguration config, ILogger<FileSystemIntentResolverBuilder> logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ManualTriggerBuildIntentResolvers()
        {
            Constants.IntentMap = FindIntentResolvers();
            Constants.InitializeWorkflows();
        }

        public void TimerTriggerBuildIntentResolvers(object? sender, ElapsedEventArgs e)
        {
            _logger.LogInformation($"{nameof(TimerTriggerBuildIntentResolvers)} started at {e.SignalTime}. Current IntentResolver count = {Constants.IntentMap.Intents.Count}");
            ManualTriggerBuildIntentResolvers();
            _logger.LogInformation($"{nameof(TimerTriggerBuildIntentResolvers)} started at {e.SignalTime}. Current IntentResolver count = {Constants.IntentMap.Intents.Count}");
        }

        public IntentMap FindIntentResolvers()
        {
            _logger.LogInformation($"{nameof(FileSystemIntentResolverBuilder)}.{nameof(FindIntentResolvers)} started.");
            var rootFolder = _config[Keys.FileSystemIntentResolverRoot];
            if (!int.TryParse(_config[Keys.FileSystemIntentResolverSearchDepth], out var searchDepth))
                searchDepth = 3;
                
            _logger.LogInformation($"Root: {rootFolder}. SearchDepth: {searchDepth}");

            // Load all distinct assemblies within the root folder
            var dllFiles = Directory
                            .EnumerateFiles(rootFolder, "*.dll", SearchOption.AllDirectories)
                            .Where(f => !f.Contains("\\obj\\")); // Exclude intermediates
            var dllMap = new Dictionary<string, Assembly>();
            foreach (var dllFile in dllFiles)
            {
                var dllName = Path.GetFileName(dllFile);
                if (!dllMap.ContainsKey(dllName))
                {
                    var assembly = Assembly.LoadFrom(dllFile);
                    dllMap.Add(dllName, assembly);
                }
            }

            // Analyze each assembly to see if it implements the IWorker<,> interface
            var intentMap = new IntentMap();
            foreach (var assembly in dllMap.Values)
            {
                var exported = assembly.GetExportedTypes();
                foreach(var exportedType in exported)
                {
                    var implementedInterfaces = exportedType.GetInterfaces();
                    foreach(var implementedInterface in implementedInterfaces)
                    {
                        var name = implementedInterface.Name;
                        var assemblyName = implementedInterface.Assembly.GetName().Name;
                        var genericParams = implementedInterface.GenericTypeArguments;
                        if (name.Contains(nameof(IWorker<string, string>)) && assemblyName == nameof(MeshApp.WorkInterface))
                        {
                            _logger.LogInformation($"{nameof(FileSystemIntentResolverBuilder)}.{nameof(FindIntentResolvers)} found intent resolver: [{exportedType.Name}]");
                            intentMap.Intents.Add(exportedType.Name.Replace("Worker", ""), new ProcessStepInfo
                            {
                                CodeType = ProcessStepInfo.Types.CodeType.CSharp,
                                FilePath = assembly.Location,
                                Name = exportedType.Name,
                                RequestType = genericParams[0].AssemblyQualifiedName,
                                ResponseType = genericParams[1].AssemblyQualifiedName,
                            });
                        }
                    }
                }
            }
            _logger.LogInformation($"{nameof(FileSystemIntentResolverBuilder)}.{nameof(FindIntentResolvers)} finished.");
            return intentMap;
        }
    }
}
