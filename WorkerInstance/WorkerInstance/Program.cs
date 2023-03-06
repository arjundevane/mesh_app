using Grpc.Net.Client;
using MeshApp.WorkerInstance.Statics;
using MeshApp.WorkStructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeshApp.WorkerInstance
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            if ((Environment.GetEnvironmentVariable("IsOverrideDynamicPort") ?? "false").ToLower() != "true")
            {
                var argsList = args.ToList();
                argsList.Add("--urls");
                argsList.Add("https://127.0.0.1:0");
                args = argsList.ToArray();
            }
            else if (Environment.GetEnvironmentVariable("OverrideDynamicPort") != null)
            {
                var argsList = args.ToList();
                argsList.Add("--urls");
                argsList.Add($"https://127.0.0.1:{Environment.GetEnvironmentVariable("OverrideDynamicPort")}");
                args = argsList.ToArray();
            }

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddGrpc();

            var app = builder.Build();
            app.MapGrpcService<WorkerService>();
            app.MapGet("/", () => "This service only supports gRPC endpoints.");

            app.Start();

            Constants.IntentMap = await RegisterSelf(app);

            await app.WaitForShutdownAsync();
        }

        private async static Task<IntentMap> RegisterSelf(WebApplication app)
        {
            try
            {
                var server = app.Services.GetRequiredService<IServer>();
                var addressFeature = server.Features.Get<IServerAddressesFeature>();

                foreach (var address in addressFeature.Addresses)
                {
                    if (!address.Contains("https"))
                        continue;

                    using var channel = GrpcChannel.ForAddress(Constants.OrchestratorUrl);
                    var client = new WorkRegistration.WorkRegistrationClient(channel);

                    var registration = new WorkerInfo
                    {
                        RegistrationKey = Constants.RegistrationKey,
                        WorkerId = Constants.WorkerGuid,
                        RpcUrl = address.Replace("127.0.0.1", "localhost"),
                    };

                    var intentMap = await client.RegisterWorkerAsync(registration);
                    return intentMap;
                }
                throw new ApplicationException("No HTTPS address found for the current service. Cannot register with Orchestrator.");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Unable to register with orchestrator using any address.", ex);
            }
        }
    }
}
