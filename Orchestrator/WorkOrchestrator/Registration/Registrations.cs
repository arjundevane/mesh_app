using Grpc.Net.Client;
using MeshApp.WorkStructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkOrchestrator.Registration
{
    public struct WorkerRegistration
    {
        public WorkerInfo WorkerInfo;
        public GrpcChannel Channel;
    }

    public class Registrations
    {
        public readonly ConcurrentDictionary<string, WorkerRegistration> _registrations;

        public Registrations()
        {
            _registrations= new ConcurrentDictionary<string, WorkerRegistration>();
        }

        public GrpcChannel? GetRandomWorkerChannel()
        {
            if (_registrations.Count == 0)
                return null;

            var workerIds = _registrations.Keys;
            var randomIndex = new Random().Next(workerIds.Count);

            var workerRegistration = _registrations[workerIds.ElementAt(randomIndex)];

            return workerRegistration.Channel;
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
    }
}
