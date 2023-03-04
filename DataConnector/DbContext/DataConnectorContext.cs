using CouchDB.Driver;
using CouchDB.Driver.Options;
using DataConnector.DbContext;

namespace MeshApp.DataConnector.DbContext
{
    public class DataConnectorContext : CouchContext
    {
        public CouchDatabase<CarDocument> Cars { get; set; }

        protected override void OnConfiguring(CouchOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseEndpoint("http://localhost:5984/")
                .EnsureDatabaseExists()
                .UseBasicAuthentication("admin", "admin");
        }
    }
}
