using Grpc.Net.Client;
using MeshApp.WorkStructure;
using System.Timers;

namespace MeshApp.WorkOrchestrator.Services
{
    public interface IRegistration
    {
        public Task<GrpcChannel?> GetRandomWorkerChannelAsync();
        public void RegisterWorker(WorkerInfo info);
        public void UnRegisterWorker(WorkerInfo info);
        /// <summary>
        /// Checks the heart beat of all currently registered workers,
        /// and removed the ones that are not responding anymore.
        /// This implements a delegate <see cref="System.Timers.ElapsedEventHandler"></see>
        /// </summary>
        public void CheckWorkerRegistrations(object? sender, ElapsedEventArgs e);
    }
}
