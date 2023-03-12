using Grpc.Net.Client;
using MeshApp.WorkStructure;
using System.Collections.Concurrent;
using System.Timers;

namespace WorkOrchestrator.Services
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

        public void CheckWorkerRegistrations(object? sender, ElapsedEventArgs e)
        {
            // TODO - Convert this to a task array and process in parallel
            // with a callback attached to each task.
            _logger.LogInformation($"{nameof(CheckWorkerRegistrations)} started at {e.SignalTime}. Current registration count = {_registrations.Count}");
            foreach(var registration in _registrations.Values)
            {
                try
                {
                    var heartBeatClient = new Worker.WorkerClient(registration.Channel);
                    var heartBeat = heartBeatClient.HeartBeat(
                        request: new Echo { Message = $"Check WorkerId:{registration.WorkerInfo.WorkerId}" });

                    if (heartBeat != null)
                        continue;
                }
                catch (Exception ex)
                {
                    // The worker has most likely shutdown, remove it from registrations.
                    _registrations.Remove(registration.WorkerInfo.WorkerId, out var removedRegistration);
                    _logger.LogWarning($"Worker not found with Id [{registration.WorkerInfo.WorkerId}]. Removing it from registration. Exception: {ex.Message}");
                }
            }
            _logger.LogInformation($"{nameof(CheckWorkerRegistrations)} started at {e.SignalTime}. New registration count = {_registrations.Count}");
        }
    }
}
