using CarService.Proto.Car;
using Grpc.Net.Client;
using MeshApp.WorkStructure.Interfaces;

namespace CarService.WorkerImplementation.GetCarsByMakeWorker
{
    [FulfilsIntent("GetCarsByMake")]
    public class GetCarsByMakeWorker : IWorker<FindCarsByMakeRequest, CarsResponse>
    {
        public async Task<CarsResponse> RunAsync(FindCarsByMakeRequest request)
        {
            // Database call wrapper as an IWorker
            using var channel = GrpcChannel.ForAddress("https://localhost:6001");
            var client = new Proto.Car.CarService.CarServiceClient(channel);

            var message = new FindCarsByMakeRequest()
            {
                Make = request.Make,
            };

            var reply = await client.FindCarsByMakeAsync(message);

            return reply;
        }
    }
}