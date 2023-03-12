using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MeshApp.Proto;
using MeshApp.WorkInterface;
using MeshApp.WorkStructure;

namespace FindEfficientCarWorker
{
    public class FindEfficientCarWorker : IWorker<GetCarEfficiencyByMakeRequest, GetCarEfficiencyByMakeResponse>
    {
        public async Task<GetCarEfficiencyByMakeResponse> RunAsync(GetCarEfficiencyByMakeRequest request)
        {
            // Get cars for the make from the orchestrator
            using var orchestratorChannel = GrpcChannel.ForAddress("https://localhost:6002");
            var orchestratorClient = new WorkOrchestration.WorkOrchestrationClient(orchestratorChannel);

            var orchestratorResponse = await orchestratorClient.InvokeIntentAsync(new WorkRequest
            {
                Intent = "GetCarsByMake",
                Message = new WorkerMessageBase
                {
                    MessageTypeName = typeof(FindCarsByMakeRequest).FullName,
                    BaseMessage = Any.Pack(new FindCarsByMakeRequest
                    {
                        Make = request.Make
                    })
                }
            });

            if (orchestratorResponse.Error != null)
            {
                throw new Exception($"Orchestrator returned an error: {orchestratorResponse.Error.ErrorMessage}");
            }

            var carsForMake = orchestratorResponse.Message.BaseMessage.Unpack<CarsResponse>();

            // For each car calculate the efficiency
            var efficiencies = new GetCarEfficiencyByMakeResponse
            {
                Make = request.Make
            };
            foreach (var car in carsForMake.Cars)
            {
                efficiencies.Efficiencies.Add(new GetCarEfficiencyByMakeResponse.Types.Efficiency
                {
                    Car = car,
                    HorsepowerPerCcDisplacement = car.Engine.Horsepower / car.Engine.Displacement
                });
            }

            return efficiencies;
        }
    }
}