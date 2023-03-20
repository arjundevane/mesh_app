using CouchDB.Driver;
using CouchDB.Driver.Options;
using MeshApp.WorkOrchestrator.Statics;

namespace MeshApp.WorkOrchestrator.DbContext
{
    public class WorkflowDbContext : CouchContext
    {
#pragma warning disable 8618 // Suppress nullable CouchDatabase since it is auto initialized
        public WorkflowDbContext(IConfiguration configuration) 
            : base(new CouchOptionsBuilder()
                        .UseEndpoint(configuration[Keys.CouchDbEndpoint])
                        .EnsureDatabaseExists()
                        .UseBasicAuthentication(configuration[Keys.CouchDbAuthUser], configuration[Keys.CouchDbAuthPass])
                        .Options)
        {
        }

        public CouchDatabase<WorkflowDocument> Workflows { get; set; }
#pragma warning restore 8618
    }
}
