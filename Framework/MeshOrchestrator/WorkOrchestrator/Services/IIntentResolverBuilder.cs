using MeshApp.WorkStructure;
using System.Timers;

namespace MeshApp.WorkOrchestrator.Services
{
    public interface IIntentResolverBuilder
    {
        public IntentMap FindIntentResolvers(string? searchLocation = null);
        public void TimerTriggerBuildIntentResolvers(object? sender, ElapsedEventArgs e);
        public void ManualTriggerBuildIntentResolvers();
    }
}
