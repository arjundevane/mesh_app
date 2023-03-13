using MeshApp.WorkOrchestrator.RpcServices;
using MeshApp.WorkOrchestrator.Statics;
using WorkOrchestrator.HostedServicePipeline;
using WorkOrchestrator.Services;

namespace MeshApp.WorkOrchestrator
{
    public static class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            // Enable IIS
            builder.WebHost.UseIIS();
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = true;
            });

            // Setup gRPC
            builder.Services.AddGrpc(configureOptions =>
            {
                configureOptions.EnableDetailedErrors = true;
            });

            // Internal services
            builder.Services.AddControllers();
            builder.Services.AddSingleton<IRegistration, Registrations>();
            builder.Services.AddSingleton<IIntentResolverBuilder, FileSystemIntentResolverBuilder>();
            builder.Services.AddHostedService<TimerWrapper>();

            var app = builder.Build();

            // Add gRPC service fulfillers
            app.MapGrpcService<WorkRegistrationService>();
            app.MapGrpcService<WorkOrchestrationService>();

            app.Run();
        }
    }
}
