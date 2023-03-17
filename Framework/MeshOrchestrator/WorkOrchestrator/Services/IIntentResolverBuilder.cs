using MeshApp.WorkStructure;
using System.Timers;

namespace MeshApp.WorkOrchestrator.Services
{
    public interface IIntentResolverBuilder
    {
        public IntentMap FindIntentResolvers();
        public void TimerTriggerBuildIntentResolvers(object? sender, ElapsedEventArgs e);
        public void ManualTriggerBuildIntentResolvers();
    }
}
