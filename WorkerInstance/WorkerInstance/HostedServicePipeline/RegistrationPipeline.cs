using Grpc.Net.Client;
using MeshApp.WorkerInstance.Statics;
using MeshApp.WorkStructure;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerInstance.HostedServicePipeline
{
    public class RegistrationPipeline : IHostedService
    {
        private readonly IServer _server;
        private readonly IHostApplicationLifetime _hostAppLifetime;
        private readonly ILogger<RegistrationPipeline> _logger;

        public RegistrationPipeline(IServer server, IHostApplicationLifetime hostApplicationLifetime, ILogger<RegistrationPipeline> logger)
        {
            _server = server;
            _hostAppLifetime = hostApplicationLifetime;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Setup the address reader to be run after application has started.
            // Since IHostedService.StartAsync is invoked before the server has fully started and bound to the addresses.
            _hostAppLifetime.ApplicationStarted.Register(async () => await RegisterSelf()).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await UnRegisterSelf();
        }

        private async Task RegisterSelf()
        {
            _logger.LogInformation($"Starting self registration with orchestrator at location: {Constants.OrchestratorUrl}");

            var addressFeature = _server.Features.Get<IServerAddressesFeature>();

            foreach (var address in addressFeature.Addresses)
            {
                if (!address.Contains("https"))
                    continue;

                _logger.LogInformation($"Found self address : {address}");
                using var channel = GrpcChannel.ForAddress(Constants.OrchestratorUrl);
                var client = new WorkRegistration.WorkRegistrationClient(channel);

                var registration = new WorkerInfo
                {
                    RegistrationKey = Constants.RegistrationKey,
                    WorkerId = Constants.WorkerGuid,
                    RpcUrl = address.Replace("127.0.0.1", "localhost"),
                };

                var intentMap = await client.RegisterWorkerAsync(registration);
                _logger.LogInformation($"Successfully self registered with Orchestrator at [{Constants.OrchestratorUrl}] with self Id [{Constants.WorkerGuid}]");
                Constants.IntentMap = intentMap;
                return;
            }
            throw new ApplicationException("No HTTPS address found for the current service. Cannot register with Orchestrator.");
        }

        private async Task UnRegisterSelf()
        {
            using var channel = GrpcChannel.ForAddress(Constants.OrchestratorUrl);
            var client = new WorkRegistration.WorkRegistrationClient(channel);

            _logger.LogInformation($"Starting self un-registration with orchestrator at location: {Constants.OrchestratorUrl}");
            var registration = new WorkerInfo
            {
                RegistrationKey = Constants.RegistrationKey,
                WorkerId = Constants.WorkerGuid
            };

            var intentMap = await client.UnRegisterWorkerAsync(registration);
            _logger.LogInformation($"Successfully self un-registered with Orchestrator at [{Constants.OrchestratorUrl}] with self Id [{Constants.WorkerGuid}]");
        }
    }
}
