using Grpc.Core;
using Grpc.Net.Client;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
using Microsoft.Extensions.Logging;

namespace MeshApp.WorkOrchestrator.Services
{
    public class WorkRegistrationService : WorkRegistration.WorkRegistrationBase
    {
        private readonly ILogger<WorkRegistrationService> _logger;

        public WorkRegistrationService(ILogger<WorkRegistrationService> logger)
        {
            _logger = logger;
        }

        public override Task<IntentMap> RegisterWorker(WorkerInfo request, ServerCallContext context)
        {
            // Validate the keys
            if (!Constants.RegistrationKeys.Contains(request.RegistrationKey))
            {
                throw new KeyNotFoundException($"Given registration key is not valid for this orchestrator.");
            }

            // Add to registration map as new client, if one exists then overwrite it
            try
            {
                Constants.Registrations.RegisterWorker(request);

                return Task.FromResult(Constants.IntentMap);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured while registaring worker with info {request}. Error: {e.Message}");
                throw;
            }
        }
    }
}
