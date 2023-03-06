using Grpc.Core;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
using Microsoft.Extensions.Logging;

namespace MeshApp.WorkOrchestrator.Services
{
    public class WorkQueueingService : WorkQueueing.WorkQueueingBase
    {
        private readonly ILogger<WorkQueueingService> _logger;

        public WorkQueueingService(ILogger<WorkQueueingService> logger)
        {
            _logger = logger;
        }

        public async override Task<WorkResponse> QueueWork(WorkRequest request, ServerCallContext context)
        {
            try
            {
                // Get random alive worker
                var workerChannel = await Constants.Registrations.GetRandomWorkerChannel();

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
                throw;
            }
        }
    }
}
