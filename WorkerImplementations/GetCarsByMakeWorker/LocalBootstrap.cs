﻿using Google.Protobuf.Reflection;
using MeshApp.Proto;

namespace GetCarsByMakeWorker
{
    // Keep this class as internal so that it can only be used
    // when locally invoking or testing the implementation.
    internal class LocalBootstrap
    {
        public async static Task Main()
        {
            var worker = new GetCarsByMakeWorker();

            var response = await worker.RunAsync(new FindCarsByMakeRequest()
            {
                Make = "audi"
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
