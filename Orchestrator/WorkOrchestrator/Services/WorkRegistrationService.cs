using Grpc.Core;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
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
            _logger.LogInformation($"{nameof(RegisterWorker)} invoked with worker Id: {request.WorkerId}");
            // Validate the keys
            if (!Constants.RegistrationKeys.Contains(request.RegistrationKey))
            {
                throw new KeyNotFoundException($"Given registration key is not valid for this orchestrator.");
            }

            // Add to registration map as new client, if one exists then overwrite it
            try
            {
                _registration.RegisterWorker(request);
                _logger.LogInformation($"{nameof(RegisterWorker)} completed for worker Id: {request.WorkerId}");
                return Task.FromResult(Constants.IntentMap);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured while registaring worker with info {request}. Error: {e.Message}");
                throw;
            }
        }

        public override Task<UnRegisterInfo> UnRegisterWorker(WorkerInfo request, ServerCallContext context)
        {
            _logger.LogInformation($"{nameof(UnRegisterWorker)} invoked with worker Id: {request.WorkerId}");
            // Validate the keys
            if (!Constants.RegistrationKeys.Contains(request.RegistrationKey))
            {
                throw new KeyNotFoundException($"Given registration key is not valid for this orchestrator.");
            }

            // Add to registration map as new client, if one exists then overwrite it
            try
            {
                _registration.UnRegisterWorker(request);
                _logger.LogInformation($"{nameof(UnRegisterWorker)} completed with worker Id: {request.WorkerId}");
                return Task.FromResult(new UnRegisterInfo
                {
                    CanUnregister = true
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occured while registaring worker with info {request}. Error: {e.Message}");
                throw;
            }
        }

        public override Task<Echo> HeartBeat(Echo request, ServerCallContext context)
        {
            return Task.FromResult(new Echo
            {
                Message = $"{nameof(WorkOrchestrator)}.{nameof(WorkRegistrationService)} Ack: {request.Message}"
            });
        }
    }
}
