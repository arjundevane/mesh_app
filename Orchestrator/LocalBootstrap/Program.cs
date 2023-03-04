using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MeshApp.Proto;
using MeshApp.WorkStructure;

namespace MeshApp.WorkOrchestrator.Bootstrap
{
    internal class LocalBootstrap
    {
        public static async Task Main()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new Worker.WorkerClient(channel);

            var message = new WorkerMessageBase
            {
                MessageTypeName = $"{typeof(FindCarByIdRequest).Namespace}.{nameof(FindCarByIdRequest)}",
                BaseMessage = Any.Pack(new FindCarByIdRequest
                {
                    CarId = "1234"
                })
            };

            var reply = await client.PerformIntendedWorkAsync(new WorkRequest
            {
                Intent = "GetCarById",
                Message = message
            });

            var typeRegistry = TypeRegistry.FromFiles(CarReflection.Descriptor, WorkStructureReflection.Descriptor);
            var protoSerializer = new Google.Protobuf.JsonFormatter(new Google.Protobuf.JsonFormatter.Settings(false, typeRegistry));

            var responseText = protoSerializer.Format(reply);

            Console.WriteLine($"Response ==> {responseText}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}