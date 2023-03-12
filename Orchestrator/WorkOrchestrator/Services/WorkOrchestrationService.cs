using Grpc.Core;
using MeshApp.WorkStructure;
using WorkOrchestrator.Registration;

namespace MeshApp.WorkOrchestrator.Services
{
    public class WorkOrchestrationService : WorkOrchestration.WorkOrchestrationBase
    {
        private readonly ILogger<WorkOrchestrationService> _logger;
        private readonly IRegistration _registration;

        public WorkOrchestrationService(ILogger<WorkOrchestrationService> logger, IRegistration registration)
        {
            _logger = logger;
            _registration = registration ?? throw new ArgumentNullException(nameof(IRegistration));
        }

        public override Task<Echo> HeartBeat(Echo request, ServerCallContext context)
        {
            return Task.FromResult(new Echo
            {
                Message = $"{nameof(WorkOrchestrator)}.{nameof(WorkOrchestrationService)} Ack: {request.Message}"
            });
        }

        public async override Task<WorkResponse> InvokeIntent(WorkRequest request, ServerCallContext context)
        {
            try
            {
                // Get random alive worker
                var workerChannel = await _registration.GetRandomWorkerChannelAsync();

                if (workerChannel == null)
                    throw new Exception("Could not find any available worker channels.");

                // Send work and await response
                var client = new Worker.WorkerClient(workerChannel);

                var response = await client.PerformIntendedWorkAsync(request);

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured while performing work: {e.Message}");
                return new WorkResponse
                {
                    Error = new Error.Error
                    {
                        ErrorCode = Error.ErrorCode.InternalError,
                        ErrorMessage = e.Message,
                        StackTrace = e.StackTrace
                    }
                };
            }
        }

        /*
        public override Task<WorkflowResponse> InvokeWorkflow(WorkflowRequest request, ServerCallContext context)
        {
            try
            {
                return Task.FromResult(new WorkflowResponse { });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while orchestration workflow: {ex.Message}");
                return Task.FromResult(new WorkflowResponse
                {
                    Error = new Error.Error
                    {
                        ErrorCode = Error.ErrorCode.InternalError,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                });
            }
        }
        */
    }
}
