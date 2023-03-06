using MeshApp.Proto;
using MeshApp.WorkStructure;
using WorkOrchestrator.Registration;

namespace MeshApp.WorkOrchestrator.Statics
{
    public static class Constants
    {
        public static string[] RegistrationKeys = new[]
        {
            "C4383160-1D18-4477-9865-4ED99A4B7B3A"
        };

        // TODO: This should be loaded from a config Url and cached
        public static IntentMap IntentMap = new();

        public enum IntentNames
        {
            GetCarById = 1,
            GetCarByName = 2,
        }

        public static void Initialize()
        {
            IntentMap = new IntentMap();
            IntentMap.Intents.Add(IntentNames.GetCarById.ToString(), new ProcessStepInfo
            {
                Name = "GetCarByIdWorker",
                CodeType = ProcessStepInfo.Types.CodeType.CSharp,
                FilePath = "C:\\Users\\arjun\\Desktop\\Mesh\\Full Implementation\\WorkerImplementations\\GetCarByIdWorker\\bin\\Debug\\net6.0\\GetCarByIdWorker.dll",
                RequestType = typeof(FindCarByIdRequest).AssemblyQualifiedName,
                ResponseType = typeof(CarResponse).AssemblyQualifiedName
            });
        }
    }
}
