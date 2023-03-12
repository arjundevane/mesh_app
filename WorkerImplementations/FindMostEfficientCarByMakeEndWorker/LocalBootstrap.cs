using Google.Protobuf.Reflection;
using MeshApp.Proto;

namespace FindMostEfficientInListWorker
{
    // Keep this class as internal so that it can only be used
    // when locally invoking or testing the implementation.
    internal class LocalBootstrap
    {
        public async static Task Main()
        {
            var worker = new FindMostEfficientCarByMakeEndWorker();


            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
