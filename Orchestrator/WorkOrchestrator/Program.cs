using MeshApp.WorkOrchestrator.Services;
using MeshApp.WorkOrchestrator.Statics;
using WorkOrchestrator.HostedServicePipeline;
using WorkOrchestrator.Registration;

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

            builder.Services.AddSingleton<IRegistration, Registrations>();

            // Setup gRPC
            builder.Services.AddGrpc(configureOptions =>
            {
                configureOptions.EnableDetailedErrors = true;
            });

            builder.Services.AddControllers();
            builder.Services.AddHostedService<TimerWrapper>();

            var app = builder.Build();

            // Add gRPC service fulfillers
            app.MapGrpcService<WorkRegistrationService>();
            app.MapGrpcService<WorkOrchestrationService>();

            Constants.Initialize();
            app.Run();
        }
    }
}
