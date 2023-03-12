using Grpc.Net.Client;
using MeshApp.WorkStructure;
using System.Collections.Concurrent;

namespace WorkOrchestrator.Registration
{
    public struct WorkerRegistration
    {
        public WorkerInfo WorkerInfo;
        public GrpcChannel Channel;
    }

    public class Registrations : IRegistration
    {
        public readonly ConcurrentDictionary<string, WorkerRegistration> _registrations;
        public readonly ILogger<Registrations> _logger;

        public Registrations(ILogger<Registrations> logger)
        {
            _registrations = new ConcurrentDictionary<string, WorkerRegistration>();
            _logger = logger;
        }

        public async Task<GrpcChannel?> GetRandomWorkerChannelAsync()
        {
            if (_registrations.Count == 0)
                return null;

            int retryCounter = 0;
            string currentWorkerId = "Unallocated";
            do
            {
                try
                {
                    var workerIds = _registrations.Keys;
                    var randomIndex = new Random().Next(workerIds.Count);
                    currentWorkerId = workerIds.ElementAt(randomIndex);

                    var workerRegistration = _registrations[workerIds.ElementAt(randomIndex)];

                    // Check heartbeat
                    var heartBeatClient = new Worker.WorkerClient(workerRegistration.Channel);
                    var heartBeat = await heartBeatClient.HeartBeatAsync(
                        request: new Echo { Message = $"Check WorkerId:{workerRegistration.WorkerInfo.WorkerId}" });

                    if (heartBeat != null)
                        return workerRegistration.Channel;
                }
                catch (Exception e)
                {
                    // The worker has most likely shutdown, remove it from registrations.
                    _registrations.Remove(currentWorkerId, out var removedRegistration);
                    _logger.LogWarning($"Worker not found with Id [{currentWorkerId}]. Removing it from registration. Exception: {e.Message}");
                }
            } while (retryCounter < _registrations.Count);

            return null;
        }

        public void RegisterWorker(WorkerInfo info)
        {
            var workRegistration = new WorkerRegistration
            {
                Channel = GrpcChannel.ForAddress(info.RpcUrl),
                WorkerInfo = info
            };
            _registrations[info.WorkerId] = workRegistration;
        }

        public void UnRegisterWorker(WorkerInfo info)
        {
            if (_registrations.ContainsKey(info.WorkerId))
            {
                _registrations.Remove(info.WorkerId, out var removedRegistration);
            }
        }
    }
}
