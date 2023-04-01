using Grpc.Core;
using MeshApp.WorkOrchestrator.Services;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
using MeshApp.WorkStructure.Error;

namespace MeshApp.WorkOrchestrator.RpcServices
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
                    Error = new Error
                    {
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = e.Message,
                        StackTrace = e.StackTrace
                    }
                };
            }
        }

        public override async Task<WorkflowResponse> InvokeWorkflow(WorkflowRequest request, ServerCallContext context)
        {
            try
            {
                // Chain the request and responses through the workflow intents
                var workRequests = new List<WorkRequest>();
                var workResponses = new List<WorkResponse>();

                if (!Constants.Workflows.TryGetValue(request.RequestedWorkflowName, out var workflowDetails) || workflowDetails == null)
                {
                    throw new EntryPointNotFoundException($"No workflow found for name: {request.RequestedWorkflowName}");
                }

                WorkRequest? lastWorkflowStepRequest = null;
                WorkResponse? lastWorkflowStepResponse = null;

                foreach (var intentStep in workflowDetails.WorkflowSteps)
                {
                    if (lastWorkflowStepRequest == null)
                    {
                        lastWorkflowStepRequest = request.FirstRequestPayload;
                    }
                    else
                    {
                        lastWorkflowStepRequest = new WorkRequest
                        {
                            Intent = intentStep.StepName,
                            Message = lastWorkflowStepResponse.Message
                        };
                    }
                    lastWorkflowStepResponse = await InvokeIntent(lastWorkflowStepRequest, context);

                    if (lastWorkflowStepResponse.Error != null)
                    {
                        throw new ApplicationException($"{nameof(InvokeWorkflow)} --> Error occured while executing step [{intentStep.StepName}]. " +
                            $"Error: [{lastWorkflowStepResponse.Error.ErrorMessage}]");
                    }

                    workRequests.Add(lastWorkflowStepRequest);
                    workResponses.Add(lastWorkflowStepResponse);
                }

                var finalResponse = new WorkflowResponse
                {
                    FinalResponsePayload = lastWorkflowStepResponse
                };
                finalResponse.WorkRequests.AddRange(workRequests);
                finalResponse.WorkResponses.AddRange(workResponses);

                return finalResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while orchestration workflow: {ex.Message}");
                return new WorkflowResponse
                {
                    Error = new Error
                    {
                        ErrorCode = ErrorCode.InternalError,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    }
                };
            }
        }
    }
}
