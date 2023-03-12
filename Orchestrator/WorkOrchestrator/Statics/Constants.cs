using MeshApp.Proto;
using MeshApp.WorkStructure;

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

        public static Dictionary<string, Workflow> Workflows = new Dictionary<string, Workflow>();

        public enum IntentNames
        {
            GetCarById = 1,
            GetCarsByMake = 2,
            FindEfficientCar = 3
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
                ResponseType = typeof(CarsResponse).AssemblyQualifiedName
            });
            IntentMap.Intents.Add(IntentNames.GetCarsByMake.ToString(), new ProcessStepInfo
            {
                Name = "GetCarsByMakeWorker",
                CodeType = ProcessStepInfo.Types.CodeType.CSharp,
                FilePath = "C:\\Users\\arjun\\Desktop\\Mesh\\Full Implementation\\WorkerImplementations\\GetCarsByMakeWorker\\bin\\Debug\\net6.0\\GetCarsByMakeWorker.dll",
                RequestType = typeof(FindCarsByMakeRequest).AssemblyQualifiedName,
                ResponseType = typeof(CarsResponse).AssemblyQualifiedName
            });
            IntentMap.Intents.Add(IntentNames.FindEfficientCar.ToString(), new ProcessStepInfo
            {
                Name = "FindEfficientCarWorker",
                CodeType = ProcessStepInfo.Types.CodeType.CSharp,
                FilePath = "C:\\Users\\arjun\\Desktop\\Mesh\\Full Implementation\\WorkerImplementations\\FindEfficientCarWorker\\bin\\Debug\\net6.0\\FindEfficientCarWorker.dll",
                RequestType = typeof(GetCarEfficiencyByMakeRequest).AssemblyQualifiedName,
                ResponseType = typeof(GetCarEfficiencyByMakeResponse).AssemblyQualifiedName
            });

            Workflows.Clear();
        }
    }
}
