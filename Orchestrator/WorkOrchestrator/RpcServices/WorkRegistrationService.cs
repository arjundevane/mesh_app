using Grpc.Core;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
using WorkOrchestrator.Services;

namespace MeshApp.WorkOrchestrator.RpcServices
{
    public class WorkRegistrationService : WorkRegistration.WorkRegistrationBase
    {
        private readonly ILogger<WorkRegistrationService> _logger;
        private readonly IRegistration _registration;
        private readonly IConfiguration _config;
        private static List<string> RegistrationKeys = new List<string>();

        public WorkRegistrationService(ILogger<WorkRegistrationService> logger, IRegistration registration, IConfiguration config)
        {
            _logger = logger;
            _registration = registration ?? throw new ArgumentNullException(nameof(IRegistration));
            _config = config;
            BuildRegistrationKeySet();
        }

        public override Task<IntentMap> RegisterWorker(WorkerInfo request, ServerCallContext context)
        {
            _logger.LogInformation($"{nameof(RegisterWorker)} invoked with worker Id: {request.WorkerId}");
            // Validate the keys
            if (!RegistrationKeys.Contains(request.RegistrationKey))
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
            if (!RegistrationKeys.Contains(request.RegistrationKey))
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

        /// <summary>
        /// This method implements the WorkRegistration.HeartBeat that can be used by worker instances to see if the
        /// orchestrator is responding or not while attempting to re-register after period of inactivity.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<Echo> HeartBeat(Echo request, ServerCallContext context)
        {
            return Task.FromResult(new Echo
            {
                Message = $"{nameof(WorkOrchestrator)}.{nameof(WorkRegistrationService)} Ack: {request.Message}"
            });
        }

        private void BuildRegistrationKeySet()
        {
            if (_config[Keys.RegistrationKeysList] == null)
                throw new KeyNotFoundException($"No registration keys found in app config.");

            var delimitedKeys = _config[Keys.RegistrationKeysList];
            RegistrationKeys = delimitedKeys.Split(',').ToList();
        }
    }
}
