using CouchDB.Driver.DependencyInjection;
using DataConnector.Services;
using MeshApp.DataConnector.DbContext;

namespace MeshApp.DataConnector
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder();

            // Add couch DB
            builder.Services.AddCouchContext<DataConnectorContext>(builder =>
            {
                builder
                    .UseEndpoint("http://localhost:5984/")
                    .EnsureDatabaseExists()
                    .UseBasicAuthentication("admin", "admin");
            });

            // Add services to the container.
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
