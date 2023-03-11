using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MeshApp.WorkerInstance.Statics;
using MeshApp.WorkInterface;
using MeshApp.WorkStructure;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;
using WorkerInstance.AssemblyLoader;

namespace MeshApp.WorkerInstance
{
    public class WorkerService : Worker.WorkerBase
    {
        private readonly ILogger<WorkerService> _logger;

        public WorkerService(ILogger<WorkerService> logger)
        {
            _logger = logger;
        }

        public override Task<Echo> HeartBeat(Echo request, ServerCallContext context)
        {
            return Task.FromResult(new Echo
            {
                Message = $"{nameof(WorkerInstance)} Ack: {request.Message}"
            });
        }

        public async override Task<WorkResponse> PerformIntendedWork(WorkRequest request, ServerCallContext context)
        {
            try
            {
                var intent = request.Intent;

                // Find the related assembly name for this intent
                if (!Constants.IntentMap.Intents.ContainsKey(intent))
                    throw new Exception($"This worker is not registered to handle the intent: [{intent}]");

                var processStepInfo = Constants.IntentMap.Intents[intent];

                // Load the work logic assembly
                var assemblyLoadContext = new AssemblyLoadContext(processStepInfo.Name, isCollectible: true);

                // Get types for logic input and output
                var inputType = System.Type.GetType(processStepInfo.RequestType) ?? typeof(object);
                var outputType = System.Type.GetType(processStepInfo.ResponseType) ?? typeof(object);

                var workerInterfaceType = typeof(IWorker<,>).MakeGenericType(new[] { inputType, outputType });

                // Make logic processor method with types defined for the intent
                var logicProcessor = FindAndInvokeMethod(
                    concreteClass: new WorkerFactory(),
                    methodName: nameof(WorkerFactory.GetWorker),
                    genericArgumentTypes: new[] { inputType, outputType },
                    methodArguments: new object[] { assemblyLoadContext, processStepInfo },
                    responseType: workerInterfaceType);
                // Processor cannot be null
                if (logicProcessor is null)
                    throw new EntryPointNotFoundException(nameof(logicProcessor));

                // Extract the work request payload and convert to inputType
                var payload = FindAndInvokeMethod(
                    concreteClass: request.Message.BaseMessage,
                    methodName: nameof(Any.Unpack),
                    genericArgumentTypes: new[] { inputType },
                    methodArguments: Array.Empty<object>(),
                    responseType: inputType);

                // Get work performed by the worker
                var outputTask = FindAndInvokeMethodAsync(
                    concreteClass: logicProcessor,
                    methodName: nameof(IWorker<object, object>.RunAsync),
                    genericArgumentTypes: null,
                    methodArguments: new object[] { payload },
                    responseType: outputType);
                // Task reference cannot be null
                if (outputTask is null)
                    throw new EntryPointNotFoundException(nameof(outputTask));

                var output = (IMessage)await outputTask;

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
                _logger.LogError(ex, $"{nameof(WorkerInstance)} Error while processing request: {ex.Message}");
                return new WorkResponse
                {
                    Error = new Error.Error
                    {
                        ErrorCode = Error.ErrorCode.InternalError,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                };
            }
        }

        private static object? FindAndInvokeMethod(object concreteClass, string methodName, System.Type[]? genericArgumentTypes, object[] methodArguments, System.Type responseType)
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

            if (methodOutput.GetType().Equals(responseType)
                || methodOutput.GetType().GetInterfaces().Any(i => i.Equals(responseType)))
                return methodOutput;
            else
                throw new InvalidOperationException($"The generated output type [{methodOutput.GetType()}] does not match expected type [{responseType}]");
        }

        private static async Task<object?> FindAndInvokeMethodAsync(object concreteClass, string methodName, System.Type[]? genericArgumentTypes, object[] methodArguments, System.Type responseType)
        {
            var methodInfo = concreteClass.GetType().GetMethod(methodName);
            if (methodInfo is null)
                throw new MissingMethodException(methodName);

            var concreteMethodInfo = (genericArgumentTypes != null && genericArgumentTypes.Length > 0)
                ? methodInfo.MakeGenericMethod(genericArgumentTypes)
                : methodInfo;

            var methodOutput = (Task?)concreteMethodInfo?.Invoke(concreteClass, methodArguments);
            if (methodOutput == null)
                return default;

            await methodOutput.ConfigureAwait(false);
            var resultProperty = methodOutput.GetType().GetProperty("Result");
            return resultProperty?.GetValue(methodOutput);
        }
    }
}
