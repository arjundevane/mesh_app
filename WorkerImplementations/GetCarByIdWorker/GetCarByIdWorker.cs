using Grpc.Net.Client;
using MeshApp.Proto;
using MeshApp.WorkInterface;

namespace GetCarByIdWorker
{
    public class GetCarByIdWorker : IWorker<FindCarByIdRequest, CarsResponse>
    {
        public async Task<CarsResponse> RunAsync(FindCarByIdRequest request)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:6001");
            var client = new CarService.CarServiceClient(channel);

            var message = new FindCarByIdRequest()
            {
                CarId = request.CarId
            };

            var reply = await client.FindCarsByIdAsync(message);

            return reply;
        }
    }
}