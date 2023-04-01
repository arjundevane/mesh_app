using MeshApp.WorkOrchestrator.DbContext;
using MeshApp.WorkOrchestrator.HostedServicePipeline;
using MeshApp.WorkOrchestrator.RpcServices;
using MeshApp.WorkOrchestrator.Services;

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
            builder.Services.AddSingleton(services =>
            {
                return new WorkflowDbContext(services.GetRequiredService<IConfiguration>());
            });

            var app = builder.Build();

            // Add gRPC service fulfillers
            app.MapGrpcService<WorkRegistrationService>();
            app.MapGrpcService<WorkOrchestrationService>();
            app.MapGrpcService<WorkflowBuilderService>();

            app.Run();
        }
    }
}
