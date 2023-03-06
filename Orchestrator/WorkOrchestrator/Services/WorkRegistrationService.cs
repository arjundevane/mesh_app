using Grpc.Core;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
using Microsoft.Extensions.Logging;
using WorkOrchestrator.Registration;

namespace MeshApp.WorkOrchestrator.Services
{
    public class WorkRegistrationService : WorkRegistration.WorkRegistrationBase
    {
        private readonly ILogger<WorkRegistrationService> _logger;
        private readonly IRegistration _registration;

        public WorkRegistrationService(ILogger<WorkRegistrationService> logger, IRegistration registration)
        {
            _logger = logger;
            _registration = registration ?? throw new ArgumentNullException(nameof(IRegistration));
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
                _registration.RegisterWorker(request);

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
