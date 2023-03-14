using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MeshApp.Proto;
using MeshApp.WorkInterface;
using MeshApp.WorkStructure;

namespace FindMostEfficientCarByMakeStartWorker
{
    [FulfilsIntent("FindMostEfficientCarByMakeStart")]
    public class FindMostEfficientCarByMakeStartWorker : IWorker<FindMostEfficientCarByMakeRequest, GetCarEfficiencyByMakeResponse>
    {
        public async Task<GetCarEfficiencyByMakeResponse> RunAsync(FindMostEfficientCarByMakeRequest request)
        {
            // Get cars for the make from the orchestrator
            using var orchestratorChannel = GrpcChannel.ForAddress("https://localhost:6002");
            var orchestratorClient = new WorkOrchestration.WorkOrchestrationClient(orchestratorChannel);

            var orchestratorResponse = await orchestratorClient.InvokeIntentAsync(new WorkRequest
            {
                Intent = "FindEfficientCar",
                Message = new WorkerMessageBase
                {
                    MessageTypeName = typeof(GetCarEfficiencyByMakeRequest).FullName,
                    BaseMessage = Any.Pack(new GetCarEfficiencyByMakeRequest
                    {
                        Make = request.Make
                    })
                }
            });

            if (orchestratorResponse.Error != null)
            {
                throw new Exception($"Orchestrator returned an error: {orchestratorResponse.Error.ErrorMessage}");
            }

            var efficiencies = orchestratorResponse.Message.BaseMessage.Unpack<GetCarEfficiencyByMakeResponse>();

            var response = new GetCarEfficiencyByMakeResponse
            {
                Make = request.Make,
            };
            response.Efficiencies.AddRange(efficiencies.Efficiencies);

            return response;
        }
    }
}