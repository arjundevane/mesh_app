using Grpc.Core;
using MeshApp.WorkStructure;
using WorkOrchestrator.Registration;

namespace MeshApp.WorkOrchestrator.Services
{
    public class WorkQueueingService : WorkQueueing.WorkQueueingBase
    {
        private readonly ILogger<WorkQueueingService> _logger;
        private readonly IRegistration _registration;

        public WorkQueueingService(ILogger<WorkQueueingService> logger, IRegistration registration)
        {
            _logger = logger;
            _registration = registration ?? throw new ArgumentNullException(nameof(IRegistration));
        }

        public async override Task<WorkResponse> QueueWork(WorkRequest request, ServerCallContext context)
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
    }
}
