using MeshApp.WorkStructure;

namespace MeshApp.WorkerInstance.Statics
{
    public static class Constants
    {
        public static string RegistrationKey = "C4383160-1D18-4477-9865-4ED99A4B7B3A";

        public static string OrchestratorUrl = "https://localhost:6002";

        public static IntentMap IntentMap = new IntentMap();

        public static string WorkerGuid = Guid.NewGuid().ToString();
    }
}
