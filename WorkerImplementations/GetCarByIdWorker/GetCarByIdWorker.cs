using Grpc.Net.Client;
using MeshApp.Proto;
using MeshApp.WorkInterface;

namespace GetCarByIdWorker
{
    public class GetCarByIdWorker : IWorker<FindCarByIdRequest, CarResponse>
    {
        public async Task<CarResponse> RunAsync(FindCarByIdRequest request)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:60514");
            var client = new CarService.CarServiceClient(channel);

            var message = new FindCarByIdRequest()
            {
                CarId = request.CarId
            };

            var reply = await client.GetCarAsync(message);

            return reply;
        }
    }
}