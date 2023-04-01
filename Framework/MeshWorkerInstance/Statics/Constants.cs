using MeshApp.WorkStructure;

namespace MeshApp.WorkerInstance.Statics
{
    public static class Constants
    {
        // TODO - These two strings need to config driven, locally available
        public static string RegistrationKey = "C4383160-1D18-4477-9865-4ED99A4B7B3A";
        public static string OrchestratorUrl = "https://localhost:6002";

        // This will be set on service startup while registering with the orchestrator
        public static IntentMap IntentMap = new IntentMap();

        // Runtime generate Guid for this instance of the worker
        public static string WorkerGuid = Guid.NewGuid().ToString();
    }
}
