using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MeshApp.Proto;
using MeshApp.WorkInterface;
using MeshApp.WorkStructure;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using WorkOrchestrator.Services;
using WorkOrchestrator.Statics;

namespace MeshApp.WorkOrchestrator
{
    public class WorkerService : Worker.WorkerBase
    {
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        public async override Task<WorkResponse> PerformIntendedWork(WorkRequest request, ServerCallContext context)
        {
            try
            {
                var intent = request.Intent;

                // Find the related assembly name for this intent
                var processStepInfo = Constants.IntentMap[intent];

                // Load the work logic assembly
                var assemblyLoadContext = new AssemblyLoadContext(processStepInfo.Name, isCollectible: true);

                // Get types for logic input and output
                var inputType = System.Type.GetType(processStepInfo.RequestType) ?? typeof(object);
                var outputType = System.Type.GetType(processStepInfo.ResponseType) ?? typeof(object);

                // Make logic processor method with types defined for the intent
                /*
                var getWorkerMethod = typeof(WorkerFactory).GetMethod(nameof(WorkerFactory.GetWorker));
                if (getWorkerMethod is null)
                    throw new MissingMethodException(nameof(WorkerFactory.GetWorker));

                var concreteMethod = getWorkerMethod.MakeGenericMethod(new[] { inputType, outputType });
                var logicProcessor = concreteMethod.Invoke(new WorkerFactory(), new object[] { assemblyLoadContext, processStepInfo });
                */

                var logicProcessor = FindAndInvokeMethod<object>(
                    concreteClass: new WorkerFactory(), 
                    methodName: nameof(WorkerFactory.GetWorker),
                    genericArgumentTypes: new[] { inputType, outputType }, 
                    methodArguments: new object[] { assemblyLoadContext, processStepInfo });
                // Processor cannot be null
                if (logicProcessor is null)
                    throw new EntryPointNotFoundException(nameof(logicProcessor));

                var workerInterfaceType = typeof(IWorker<,>).MakeGenericType(new[] { inputType, outputType });

                // Get work performed by the worker
                /*
                var outputMethod = workerInterfaceType.GetMethod(nameof(IWorker<object, object>.RunAsync));
                var payload = request.Message.BaseMessage.Unpack<FindCarByIdRequest>(); // --> HARDCODED = Make it generic
                var outputTask = (Task<CarResponse>)outputMethod.Invoke(logicProcessor, new[] { payload });
                */
                var payload = request.Message.BaseMessage.Unpack<FindCarByIdRequest>(); // --> HARDCODED = Make it generic
                var outputTask = FindAndInvokeMethod<Task<CarResponse>>(
                    concreteClass: logicProcessor,
                    methodName: nameof(IWorker<object, object>.RunAsync),
                    genericArgumentTypes: null,
                    methodArguments: new object[] { payload });
                // Task reference cannot be null
                if (outputTask is null)
                    throw new EntryPointNotFoundException(nameof(outputTask));

                var output = await outputTask;

                var workResponse = new WorkResponse()
                {
                    FinishedIntent = request.Intent,
                    Message = new WorkerMessageBase
                    {
                        MessageTypeName = outputType.FullName,
                        BaseMessage = Any.Pack(output)
                    }
                };
                return workResponse;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                throw;
            }
        }

        private T? FindAndInvokeMethod<T>(object concreteClass, string methodName, System.Type[]? genericArgumentTypes, object[] methodArguments)
        {
            var methodInfo = concreteClass.GetType().GetMethod(methodName);
            if (methodInfo is null)
                throw new MissingMethodException(methodName);

            var concreteMethodInfo = (genericArgumentTypes != null && genericArgumentTypes.Length > 0)
                ? methodInfo.MakeGenericMethod(genericArgumentTypes)
                : methodInfo;

            var methodOutput = concreteMethodInfo.Invoke(concreteClass, methodArguments);
            if (methodOutput == null)
                return default;

            return (T)methodOutput;
        }
    }
}
