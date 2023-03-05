using Grpc.Net.Client;
using MeshApp.WorkerInstance.Statics;
using MeshApp.WorkStructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace MeshApp.WorkerInstance
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddGrpc();

            var app = builder.Build();

            app.MapGrpcService<WorkerService>();
            app.MapGet("/", () => "This service only supports gRPC endpoints.");

            var url = "https://localhost:5101";
            Constants.IntentMap = await RegisterSelf(url);

            app.Run(url);
        }

        public async static Task<IntentMap> RegisterSelf(string url)
        {
            try
            {
                using var channel = GrpcChannel.ForAddress(Constants.OrchestratorUrl);
                var client = new WorkRegistration.WorkRegistrationClient(channel);

                var registration = new WorkerInfo
                {
                    RegistrationKey = Constants.RegistrationKey,
                    WorkerId = Constants.WorkerGuid,
                    RpcUrl = url,
                };

                var intentMap = await client.RegisterWorkerAsync(registration);
                return intentMap;
            }
            catch(Exception ex)
            {
                throw new ApplicationException("Unable to register with orchestrator using any address.", ex);
            }
        }
    }
}
