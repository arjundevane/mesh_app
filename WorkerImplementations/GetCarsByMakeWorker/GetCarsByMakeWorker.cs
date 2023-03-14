using Grpc.Net.Client;
using MeshApp.Proto;
using MeshApp.WorkInterface;

namespace GetCarsByMakeWorker
{
    [FulfilsIntent("GetCarsByMake")]
    public class GetCarsByMakeWorker : IWorker<FindCarsByMakeRequest, CarsResponse>
    {
        public async Task<CarsResponse> RunAsync(FindCarsByMakeRequest request)
        {
            // Database call wrapper as an IWorker
            using var channel = GrpcChannel.ForAddress("https://localhost:6001");
            var client = new CarService.CarServiceClient(channel);

            var message = new FindCarsByMakeRequest()
            {
                Make = request.Make,
            };

            var reply = await client.FindCarsByMakeAsync(message);

            return reply;
        }
    }
}