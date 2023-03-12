using Grpc.Net.Client;
using MeshApp.WorkStructure;

namespace WorkOrchestrator.Registration
{
    public interface IRegistration
    {
        public Task<GrpcChannel?> GetRandomWorkerChannelAsync();
        public void RegisterWorker(WorkerInfo info);
        public void UnRegisterWorker(WorkerInfo info);
    }
}
