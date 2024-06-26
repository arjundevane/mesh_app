using CarService.DataConnector.DbContext;
using CarService.DataConnector.Services;
using CouchDB.Driver.DependencyInjection;

namespace CarService.DataConnector
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder();

            // Enable IIS
            builder.WebHost.UseIIS();
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = true;
            });

            // Add couch DB
            builder.Services.AddCouchContext<DataConnectorContext>(builder =>
            {
                builder
                    .UseEndpoint("http://localhost:5984/")
                    .EnsureDatabaseExists()
                    .UseBasicAuthentication("admin", "admin");
            });

            builder.Services.AddControllers();

            // Setup gRPC
            builder.Services.AddGrpc(configureOptions =>
            {
                configureOptions.EnableDetailedErrors = true;
            });

            var app = builder.Build();

            // Add gRPC service fulfillers
            app.MapGrpcService<DataConnectorService>();
            app.MapControllers();

            app.Run();
        }
    }
}
