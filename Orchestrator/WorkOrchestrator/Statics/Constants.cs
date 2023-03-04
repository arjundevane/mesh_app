using MeshApp.Proto;
using MeshApp.WorkStructure;

namespace WorkOrchestrator.Statics
{
    // TODO: This should be loaded from a config Url and cached
    public static class Constants
    {
        public static Dictionary<string, ProcessStepInfo> IntentMap = new()
        {
            {
                Intents.GetCarById.ToString(),
                new ProcessStepInfo
                {
                    Name = "GetCarByIdWorker",
                    CodeType = ProcessStepInfo.Types.CodeType.CSharp,
                    FilePath = "C:\\Users\\arjun\\Desktop\\Mesh\\Full Implementation\\WorkerImplementations\\GetCarByIdWorker\\bin\\Debug\\net6.0\\GetCarByIdWorker.dll",
                    RequestType = typeof(FindCarByIdRequest).AssemblyQualifiedName,
                    ResponseType = typeof(CarResponse).AssemblyQualifiedName
                }
            }
        };
    }

    public enum Intents
    {
        GetCarById = 1,
        GetCarByName = 2,
    }
}
