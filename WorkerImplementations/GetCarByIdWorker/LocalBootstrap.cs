using Google.Protobuf.Reflection;
using MeshApp.Proto;

namespace GetCarByIdWorker
{
    // Keep this class as internal so that it can only be used
    // when locally invoking or testing the implementation.
    internal class LocalBootstrap
    {
        public async static Task Main()
        {
            var worker = new GetCarByIdWorker();

            var response = await worker.RunAsync(new FindCarByIdRequest()
            {
                CarId = "1234"
            }); 

            var typeRegistry = TypeRegistry.FromFiles(CarReflection.Descriptor);
            var protoSerializer = new Google.Protobuf.JsonFormatter(new Google.Protobuf.JsonFormatter.Settings(false, typeRegistry));

            var responseText = protoSerializer.Format(response);

            Console.WriteLine($"Response ==> {responseText}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
