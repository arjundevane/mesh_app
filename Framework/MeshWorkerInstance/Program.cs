using MeshApp.WorkerInstance.HostedServicePipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MeshApp.WorkerInstance
{
    public static class Program
    {
        public async static Task Main(string[] args)
        {
            // Port manipulation by simulating input args
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
            builder.Services.AddLogging();
            builder.Services.AddHostedService<RegistrationPipeline>();

            var app = builder.Build();
            app.MapGrpcService<WorkerService>();

            app.Start();

            await app.WaitForShutdownAsync();
        }
    }
}
