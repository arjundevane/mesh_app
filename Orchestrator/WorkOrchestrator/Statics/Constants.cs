using MeshApp.Proto;
using MeshApp.WorkStructure;

namespace MeshApp.WorkOrchestrator.Statics
{
    public static class Constants
    {
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

        public static void InitializeWorkflows()
        {
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

    public static class Keys
    {
        public const string RegistrationKeysList = nameof(RegistrationKeysList);
        public const string FileSystemIntentResolverRoot = nameof(FileSystemIntentResolverRoot);
        public const string FileSystemIntentResolverSearchDepth = nameof(FileSystemIntentResolverSearchDepth);
    }
}
