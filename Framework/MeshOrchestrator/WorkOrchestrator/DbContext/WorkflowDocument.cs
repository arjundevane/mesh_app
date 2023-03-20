using CouchDB.Driver.Types;
using MeshApp.WorkStructure;
using Newtonsoft.Json;

namespace MeshApp.WorkOrchestrator.DbContext
{
    [JsonObject("workflow")]
    public class WorkflowDocument : CouchDocument
    {
        public WorkflowDocument()
        {
            Workflow = new Workflow();
        }
        public Workflow Workflow { get; set; }
    }
}
