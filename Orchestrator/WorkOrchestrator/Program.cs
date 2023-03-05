using MeshApp.WorkOrchestrator.Services;
using MeshApp.WorkOrchestrator.Statics;
using MeshApp.WorkStructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MeshApp.WorkOrchestrator
{
    public static class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddGrpc();

            var app = builder.Build();

            app.MapGrpcService<WorkRegistrationService>();
            app.MapGrpcService<WorkQueueingService>();
            app.MapGet("/", () => "This service only supports gRPC endpoints.");

            Constants.Initialize();
            app.Run();
        }
    }
}
