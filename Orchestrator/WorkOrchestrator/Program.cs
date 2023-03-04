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

            app.MapGrpcService<WorkerService>();
            app.MapGet("/", () => "This service only supports gRPC endpoints.");

            app.Run();
        }
    }
}
