using Grpc.Net.Client;
using CarService.Proto.Car;
using MeshApp.WorkStructure.Interfaces;

namespace CarService.WorkerImplementation.GetCarByIdWorker
{
    [FulfilsIntent("GetCarById")]
    public class GetCarByIdWorker : IWorker<FindCarByIdRequest, CarsResponse>
    {
        public async Task<CarsResponse> RunAsync(FindCarByIdRequest request)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:6001");
            var client = new Proto.Car.CarService.CarServiceClient(channel);

            var message = new FindCarByIdRequest()
            {
                CarId = request.CarId
            };

            var reply = await client.FindCarsByIdAsync(message);

            return reply;
        }
    }
}