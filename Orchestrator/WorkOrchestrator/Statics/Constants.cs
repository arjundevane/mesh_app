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
            FindEfficientCar = 3,
            FindMostEfficientCarByMakeStart = 4,
            FindMostEfficientCarByMakeEnd = 5
        }

        public static void Initialize()
        {
            // Build one step intents
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
            IntentMap.Intents.Add(IntentNames.FindMostEfficientCarByMakeStart.ToString(), new ProcessStepInfo
            {
                Name = "FindMostEfficientCarByMakeStartWorker",
                CodeType = ProcessStepInfo.Types.CodeType.CSharp,
                FilePath = "C:\\Users\\arjun\\Desktop\\Mesh\\Full Implementation\\WorkerImplementations\\FindMostEfficientCarByMakeStartWorker\\bin\\Debug\\net6.0\\FindMostEfficientCarByMakeStartWorker.dll",
                RequestType = typeof(FindMostEfficientCarByMakeRequest).AssemblyQualifiedName,
                ResponseType = typeof(GetCarEfficiencyByMakeResponse).AssemblyQualifiedName
            });
            IntentMap.Intents.Add(IntentNames.FindMostEfficientCarByMakeEnd.ToString(), new ProcessStepInfo
            {
                Name = "FindMostEfficientCarByMakeEndWorker",
                CodeType = ProcessStepInfo.Types.CodeType.CSharp,
                FilePath = "C:\\Users\\arjun\\Desktop\\Mesh\\Full Implementation\\WorkerImplementations\\FindMostEfficientCarByMakeEndWorker\\bin\\Debug\\net6.0\\FindMostEfficientCarByMakeEndWorker.dll",
                RequestType = typeof(GetCarEfficiencyByMakeResponse).AssemblyQualifiedName,
                ResponseType = typeof(FindMostEfficientCarByMakeResponse).AssemblyQualifiedName
            });

            // Build workflows
            Workflows.Clear();

            Workflow FindMostEfficientCarByMakeWorkflow = new Workflow
            {
                WorkflowId = "96DA70EA-F95E-49DD-8591-36C9520E0833",
                WorkflowName = nameof(FindMostEfficientCarByMakeWorkflow),
            };
            FindMostEfficientCarByMakeWorkflow.WorkflowSteps.Add(new WorkflowStep
            {
                StepId = "06E3D5BD-4107-4CDB-AB00-2D9A304CBC64",
                StepName = IntentNames.FindMostEfficientCarByMakeStart.ToString(),
                SingleStep = IntentMap.Intents[IntentNames.FindMostEfficientCarByMakeStart.ToString()]
            });
            FindMostEfficientCarByMakeWorkflow.WorkflowSteps.Add(new WorkflowStep
            {
                StepId = "E381D73F-C0A1-4052-BBC0-6AF64A2520A0",
                StepName = IntentNames.FindMostEfficientCarByMakeEnd.ToString(),
                SingleStep = IntentMap.Intents[IntentNames.FindMostEfficientCarByMakeEnd.ToString()]
            });

            Workflows.Add(nameof(FindMostEfficientCarByMakeWorkflow), FindMostEfficientCarByMakeWorkflow);
        }
    }
}
